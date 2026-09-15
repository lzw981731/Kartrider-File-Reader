use p5136_rho5::{
    P5136_PACKED_ENTRY_FLAGS, Rho5Directory, Rho5Limits, Rho5Region, Rho5WriteEntry, Rho5Writer,
};
use std::ffi::{CStr, CString};
use std::os::raw::{c_char, c_int, c_void};
use std::path::PathBuf;
use std::ptr;
use std::slice;

/// FFI 句柄：封装一次扫描的目录索引，跨多次 extract/replace 复用。
pub struct Rho5FfiHandle {
    directory: Rho5Directory,
}

/// 与 C# 侧对应的条目元数据（固定布局，方便 marshaling）。
#[repr(C)]
pub struct Rho5FfiEntry {
    /// 内部路径（UTF-8, NUL 结尾）；Rust 侧分配，调用者用 rho5_free_string 释放
    pub path: *mut c_char,
    /// 所属归档文件名（如 DataPack1_00000.rho5）
    pub archive: *mut c_char,
    /// 压缩后大小
    pub compressed_size: i64,
    /// 解压后大小
    pub plaintext_size: i64,
}

// ---------------------------------------------------------------------------
// 错误处理
// ---------------------------------------------------------------------------
static LAST_ERROR: std::sync::Mutex<Option<String>> = std::sync::Mutex::new(None);

fn set_last_error(message: impl Into<String>) {
    if let Ok(mut guard) = LAST_ERROR.lock() {
        *guard = Some(message.into());
    }
}

fn take_last_error() -> String {
    if let Ok(mut guard) = LAST_ERROR.lock() {
        guard.take().unwrap_or_else(|| "".to_owned())
    } else {
        String::new()
    }
}

/// 取得最近一次错误信息（UTF-8），写入 buffer；返回需要的长度（不含 NUL）。
#[unsafe(no_mangle)]
pub extern "C" fn rho5_last_error(buf: *mut c_char, buf_cap: i32) -> i32 {
    let message = take_last_error();
    let bytes = message.as_bytes();
    let cap = buf_cap.max(0) as usize;
    if cap > 0 && !buf.is_null() {
        let out = unsafe { slice::from_raw_parts_mut(buf as *mut u8, cap) };
        let n = bytes.len().min(cap - 1);
        out[..n].copy_from_slice(&bytes[..n]);
        out[n] = 0;
    }
    bytes.len() as i32 + 1
}

// ---------------------------------------------------------------------------
// 句柄生命周期
// ---------------------------------------------------------------------------
/// 打开 Data 目录（KR 区）。返回句柄；失败返回 NULL（用 rho5_last_error 取信息）。
#[unsafe(no_mangle)]
pub extern "C" fn rho5_open(data_dir: *const c_char) -> *mut c_void {
    let dir = match unsafe { CStr::from_ptr(data_dir) }.to_str() {
        Ok(s) => s,
        Err(e) => {
            set_last_error(format!("data_dir 不是合法 UTF-8: {e}"));
            return ptr::null_mut();
        }
    };
    let limits = Rho5Limits::default();
    match Rho5Directory::scan_kr(dir, limits) {
        Ok(directory) => {
            let handle = Box::new(Rho5FfiHandle { directory });
            Box::into_raw(handle) as *mut c_void
        }
        Err(e) => {
            set_last_error(e.to_string());
            ptr::null_mut()
        }
    }
}

/// 释放句柄。
#[unsafe(no_mangle)]
pub extern "C" fn rho5_close(handle: *mut c_void) {
    if handle.is_null() {
        return;
    }
    unsafe {
        drop(Box::from_raw(handle as *mut Rho5FfiHandle));
    }
}

// ---------------------------------------------------------------------------
// 条目枚举
// ---------------------------------------------------------------------------
/// 返回条目数组。*out_entries 指向 C 数组（Rust 分配），*out_count 为数组长度。
/// 释放方式：rho5_free_entries。
#[unsafe(no_mangle)]
pub extern "C" fn rho5_list(handle: *mut c_void, out_entries: *mut *mut Rho5FfiEntry, out_count: *mut i64) -> c_int {
    let h = handle as *const Rho5FfiHandle;
    let h = match unsafe { h.as_ref() } {
        Some(h) => h,
        None => {
            set_last_error("rho5_list: 句柄为空");
            return -1;
        }
    };
    let entries = h.directory.entries();
    let mut ffi: Vec<Rho5FfiEntry> = Vec::with_capacity(entries.len());
    for e in entries {
        let path = match CString::new(e.normalized_path()) {
            Ok(p) => p,
            Err(_) => continue,
        };
        let archive = match CString::new(e.archive_name()) {
            Ok(a) => a,
            Err(_) => continue,
        };
        ffi.push(Rho5FfiEntry {
            path: path.into_raw(),
            archive: archive.into_raw(),
            compressed_size: e.compressed_size() as i64,
            plaintext_size: e.plaintext_size() as i64,
        });
    }
    let count = ffi.len() as i64;
    let boxed = Box::new(ffi);
    unsafe {
        *out_entries = Box::into_raw(boxed) as *mut Rho5FfiEntry;
        *out_count = count;
    }
    0
}

/// 释放 rho5_list 返回的条目数组。
#[unsafe(no_mangle)]
pub extern "C" fn rho5_free_entries(entries: *mut Rho5FfiEntry, count: i64) {
    if entries.is_null() || count <= 0 {
        return;
    }
    unsafe {
        let boxed: Box<Vec<Rho5FfiEntry>> = Box::from_raw(entries as *mut Vec<Rho5FfiEntry>);
        for entry in boxed.iter() {
            if !entry.path.is_null() {
                drop(CString::from_raw(entry.path));
            }
            if !entry.archive.is_null() {
                drop(CString::from_raw(entry.archive));
            }
        }
    }
}

// ---------------------------------------------------------------------------
// 提取
// ---------------------------------------------------------------------------
/// 提取单个文件。成功返回 0，*out_data 指向 malloc 的缓冲区（用 rho5_free_bytes 释放），
/// *out_len 为长度。失败返回非 0（rho5_last_error）。
#[unsafe(no_mangle)]
pub extern "C" fn rho5_extract(
    handle: *mut c_void,
    path: *const c_char,
    out_data: *mut *mut u8,
    out_len: *mut i64,
) -> c_int {
    let h = match unsafe { (handle as *const Rho5FfiHandle).as_ref() } {
        Some(h) => h,
        None => {
            set_last_error("rho5_extract: 句柄为空");
            return -1;
        }
    };
    let path = match unsafe { CStr::from_ptr(path) }.to_str() {
        Ok(s) => s,
        Err(e) => {
            set_last_error(format!("path 不是合法 UTF-8: {e}"));
            return -1;
        }
    };
    match h.directory.extract_entry_with_legacy_padding(h.directory.unique_entry(path).unwrap_or_else(|_| panic!("{path}"))) {
        Ok(data) => {
            let data_box = data.into_boxed_slice();
            let len = data_box.len() as i64;
            let raw = Box::into_raw(data_box) as *mut u8;
            unsafe {
                *out_data = raw;
                *out_len = len;
            }
            0
        }
        Err(e) => {
            set_last_error(e.to_string());
            -1
        }
    }
}

/// 释放 rho5_extract 返回的缓冲区。
#[unsafe(no_mangle)]
pub extern "C" fn rho5_free_bytes(ptr: *mut u8, len: i64) {
    if ptr.is_null() || len <= 0 {
        return;
    }
    unsafe {
        let slice = slice::from_raw_parts_mut(ptr, len as usize);
        drop(Box::from_raw(slice as *mut [u8]));
    }
}

// ---------------------------------------------------------------------------
// 替换 / 新增
// ---------------------------------------------------------------------------
/// 替换（或新增）条目。把 data 写回目标条目所在归档，重编码后落盘（写前备份 .bak）。
/// 成功返回 0；失败返回非 0。
#[unsafe(no_mangle)]
pub extern "C" fn rho5_replace(
    handle: *mut c_void,
    path: *const c_char,
    data: *const u8,
    len: i64,
) -> c_int {
    let h = match unsafe { (handle as *const Rho5FfiHandle).as_ref() } {
        Some(h) => h,
        None => {
            set_last_error("rho5_replace: 句柄为空");
            return -1;
        }
    };
    let path = match unsafe { CStr::from_ptr(path) }.to_str() {
        Ok(s) => s,
        Err(e) => {
            set_last_error(format!("path 不是合法 UTF-8: {e}"));
            return -1;
        }
    };
    if len < 0 || (len > 0 && data.is_null()) {
        set_last_error("rho5_replace: data 无效");
        return -1;
    }
    let new_data = if len == 0 {
        Vec::new()
    } else {
        unsafe { slice::from_raw_parts(data, len as usize) }.to_vec()
    };
    let directory = h.directory.directory();

    // 找到目标条目（存在则替换；不存在则当作新增——追加到第一个归档）
    let target_archive_name: String;
    let target_archive_path: PathBuf;
    let mut entries_to_write: Vec<(String, Vec<u8>, i32)> = Vec::new();
    let mut replaced = false;

    match h.directory.unique_entry(path) {
        Ok(entry) => {
            target_archive_name = entry.archive_name().to_owned();
            target_archive_path = entry.archive_path().to_owned();
            // 收集该归档所有条目
            for e in h.directory.entries() {
                if e.archive_name() == target_archive_name {
                    if e.normalized_path() == path {
                        entries_to_write.push((path.to_owned(), new_data.clone(), P5136_PACKED_ENTRY_FLAGS));
                        replaced = true;
                    } else {
                        match h.directory.extract_entry_with_legacy_padding(e) {
                            Ok(bytes) => entries_to_write.push((
                                e.normalized_path().to_owned(),
                                bytes,
                                e.flags(),
                            )),
                            Err(err) => {
                                set_last_error(format!("读取 {path} 所在归档成员失败: {err}"));
                                return -1;
                            }
                        }
                    }
                }
            }
        }
        Err(_) => {
            // 新增：追加到第一个归档（与客户端扫描顺序一致）
            let first_archive = h.directory.entries().first().map(|e| e.archive_name().to_owned());
            match first_archive {
                Some(name) => {
                    target_archive_name = name.clone();
                    target_archive_path = directory.join(&name);
                    for e in h.directory.entries() {
                        if e.archive_name() == name {
                            match h.directory.extract_entry_with_legacy_padding(e) {
                                Ok(bytes) => entries_to_write.push((
                                    e.normalized_path().to_owned(),
                                    bytes,
                                    e.flags(),
                                )),
                                Err(err) => {
                                    set_last_error(format!("读取归档成员失败: {err}"));
                                    return -1;
                                }
                            }
                        }
                    }
                    entries_to_write.push((path.to_owned(), new_data, P5136_PACKED_ENTRY_FLAGS));
                }
                None => {
                    set_last_error("Data 目录中没有 rho5 归档，无法新增条目");
                    return -1;
                }
            }
        }
    }

    // 用 Rho5Writer 重编码
    let mut writer = Rho5Writer::new();
    for (p, bytes, flags) in entries_to_write {
        writer.add(Rho5WriteEntry {
            path: p,
            data: bytes,
            flags,
        });
    }
    let limits = Rho5Limits::default();
    let encoded = match writer.encode(&target_archive_name, Rho5Region::Korea, &limits) {
        Ok(e) => e,
        Err(err) => {
            set_last_error(format!("重编码 {target_archive_name} 失败: {err}"));
            return -1;
        }
    };

    // 备份 + 写回
    let backup = PathBuf::from(format!("{}.bak", target_archive_path.display()));
    if !backup.exists() {
        if let Err(err) = std::fs::copy(&target_archive_path, &backup) {
            set_last_error(format!("备份 {target_archive_path:?} 失败: {err}"));
            return -1;
        }
    }
    if let Err(err) = std::fs::write(&target_archive_path, encoded.as_bytes()) {
        set_last_error(format!("写回 {target_archive_path:?} 失败: {err}"));
        return -1;
    }
    let _ = replaced;
    0
}