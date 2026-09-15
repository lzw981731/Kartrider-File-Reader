//! rho5tool: 用 P5136_Rust (p5136-rho5) 读写 RHO5 归档的命令行工具。
//!
//! 用法:
//!   rho5tool list    <Data目录> [过滤子串]
//!   rho5tool extract <Data目录> <内部路径> <输出文件>
//!   rho5tool add     <Data目录> <内部路径> <新文件>      # 写入/新增（P5136 实现）

use p5136_rho5::{
    P5136_PACKED_ENTRY_FLAGS, Rho5Directory, Rho5Limits, Rho5Region, Rho5WriteEntry, Rho5Writer,
};
use std::env;

fn usage() {
    eprintln!("用法:");
    eprintln!("  rho5tool list    <Data目录> [过滤子串]");
    eprintln!("  rho5tool extract <Data目录> <内部路径> <输出文件>");
    eprintln!("  rho5tool add     <Data目录> <内部路径> <新文件>");
}

fn main() {
    let args: Vec<String> = env::args().collect();
    let cmd = args.get(1).map(String::as_str).unwrap_or("");

    match cmd {
        "list" => {
            let data_dir = need(&args, 2, "Data目录");
            let filter = args.get(3).cloned().unwrap_or_default();
            match Rho5Directory::scan_kr(data_dir, Rho5Limits::default()) {
                Ok(d) => {
                    println!("归档数: {} 条目数: {}", d.archive_count(), d.entries().len());
                    for e in d.entries() {
                        let p = e.normalized_path();
                        if filter.is_empty() || p.contains(&filter) {
                            println!(
                                "[{}] {} ({} -> {})",
                                e.archive_name(),
                                p,
                                e.compressed_size(),
                                e.plaintext_size()
                            );
                        }
                    }
                }
                Err(err) => exit_err(&format!("扫描失败: {err}")),
            }
        }
        "extract" => {
            let data_dir = need(&args, 2, "Data目录");
            let path = need(&args, 3, "内部路径");
            let out = need(&args, 4, "输出文件");
            match Rho5Directory::scan_kr(data_dir, Rho5Limits::default()) {
                Ok(d) => {
                    let entry = match d.unique_entry(path) {
                        Ok(e) => e,
                        Err(err) => exit_err(&format!("条目不存在 {path}: {err}")),
                    };
                    match d.extract_entry_with_legacy_padding(entry) {
                        Ok(data) => {
                            std::fs::write(out, &data).expect("写入输出文件失败");
                            println!("已提取 {path} -> {out} ({} bytes)", data.len());
                        }
                        Err(err) => exit_err(&format!("提取失败: {err}")),
                    }
                }
                Err(err) => exit_err(&format!("扫描失败: {err}")),
            }
        }
        "add" => {
            let data_dir = need(&args, 2, "Data目录");
            let target_path = need(&args, 3, "内部路径(如 etc_/itemTable@kr.xml)");
            let new_file = need(&args, 4, "新文件路径");
            let data = std::fs::read(new_file).unwrap_or_else(|e| exit_err(&format!("读取新文件失败: {e}")));
            let limits = Rho5Limits::default();
            let dir = Rho5Directory::scan_kr(data_dir, limits.clone()).unwrap_or_else(|e| exit_err(&format!("扫描失败: {e}")));

            // 确定目标归档：已存在则替换到原归档，否则新增到第一个归档
            let target_archive: String;
            let mut replaced = false;
            match dir.unique_entry(target_path) {
                Ok(entry) => {
                    target_archive = entry.archive_name().to_owned();
                    replaced = true;
                }
                Err(_) => {
                    target_archive = match dir.entries().first() {
                        Some(e) => e.archive_name().to_owned(),
                        None => exit_err("Data 目录没有 rho5 归档"),
                    };
                }
            }

            // 收集目标归档全部成员（目标条目换成新数据）
            let mut rebuilt: Vec<(String, Vec<u8>, i32)> = Vec::new();
            for e in dir.entries() {
                if e.archive_name() != target_archive {
                    continue;
                }
                if e.normalized_path() == target_path {
                    rebuilt.push((target_path.to_owned(), data.clone(), P5136_PACKED_ENTRY_FLAGS));
                } else {
                    match dir.extract_entry_with_legacy_padding(e) {
                        Ok(bytes) => rebuilt.push((e.normalized_path().to_owned(), bytes, e.flags())),
                        Err(err) => exit_err(&format!("读取归档成员失败: {err}")),
                    }
                }
            }

            let mut writer = Rho5Writer::new();
            for (p, bytes, flags) in &rebuilt {
                writer.add(Rho5WriteEntry {
                    path: p.clone(),
                    data: bytes.clone(),
                    flags: *flags,
                });
            }
            let encoded = match writer.encode(&target_archive, Rho5Region::Korea, &limits) {
                Ok(e) => e,
                Err(err) => exit_err(&format!("重编码失败: {err}")),
            };

            // 定位旧归档完整路径并备份写回
            let archive_path = dir
                .entries()
                .iter()
                .find(|e| e.archive_name() == target_archive)
                .map(|e| e.archive_path().to_owned())
                .unwrap_or_else(|| std::path::PathBuf::from(&target_archive));
            let backup = std::path::PathBuf::from(format!("{}.bak", archive_path.display()));
            if !backup.exists() {
                std::fs::copy(&archive_path, &backup).unwrap_or_else(|e| exit_err(&format!("备份原归档失败: {e}")));
            }
            std::fs::write(&archive_path, encoded.as_bytes()).unwrap_or_else(|e| exit_err(&format!("写回归档失败: {e}")));
            println!(
                "{} {target_path} <- {new_file} ({} bytes) 到 {}（备份: {}）",
                if replaced { "已替换" } else { "已新增" },
                data.len(),
                archive_path.display(),
                backup.display()
            );
        }
        _ => {
            usage();
            std::process::exit(64);
        }
    }
}

fn need<'a>(args: &'a [String], idx: usize, name: &str) -> &'a str {
    match args.get(idx) {
        Some(v) => v,
        None => {
            eprintln!("缺少参数: {name}");
            usage();
            std::process::exit(64);
        }
    }
}

fn exit_err(message: &str) -> ! {
    eprintln!("{message}");
    std::process::exit(1);
}