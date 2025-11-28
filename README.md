KartCityLite (formerly known as RhoLoader and JmdLoader)
====================

Introduction
----------------------
KartCityLite is a tool for reading KartRider and Raycity game file.
If you encountered some problems while using this program, you can submit your problem on RhoReader Github Page.
Thank you for using KartCityLite.

Requirment
----------------------
OS: Windows 7 SP1 and least version
CPU: x64 Processor with SSE instruction set support or ARM64 Processor with Neon instruction set support.

Release Note
----------------------
### DevVersion
#### Implemented
* Add **Neon instruction set** support for the ARM64 architecture to encode and decode Rho5-Type files.
* Now you can open multiple Rho and Jmd files via ``File > Open`` option.
* Change the rule of Rho path so that extracted folders and files can be used as DataRaw of KartRider.
* Merge common parts of ``KartLibrary`` and ``RaycityLibrary`` into ``KartCity.Common`` library.
* Add support for previewing XML.
  On Windows, XML files are opened in the default browser by default, but this is too0000 slow, so a built-in XML preview has been added.
* Enhance the performance of BML previewing and Discard RichTextBox to enhance performance.
* Use Noto Sans CJK font in Korean, Simplified Chinese and Traditional Chinese.
* Use asynchronous file loading to ensure KartCityLite will not "Not responding".
* Improve **Preivew Image** feature.
* Add preview pane.
* Merge ``RhoLoader`` and ``JmdLoader`` into KartCityLite. (It means that you can open both KartRider and Raycity game files via this program.)

#### In progress
* Add **Preview Track** feature.
* Add **Preview Music** feature.
* Add **Modifiy File** feature. Currently, only ``RhoArchive`` and ``Rho5Archive`` support file modification, ``KartStorageSystem`` is in progress.

### 23.1.31
* Add Rh layer 1.0 support.
* Add Rho5 support (it is only available in "Open Data Folder")
* Add Korean support.
* Add "Check update" feature.
* Add "Open Data Folder" feature, you can read all of Rho and Rho5 files in "Data" folder via this new feature.
* Remove some unused options.
* Fix some bugs.
* Improve UI design.
* Improve data structure of "KartRider File Library".


### 21.1.3
* First version

介紹
----------------------
KartCityLite(原RhoLoader、JmdLoader)可以開啟跑跑卡丁車與光速城市的遊戲檔案。
如果您在運行KartCityLite時有遇到些問題，您可以提交您的問題到本程式的Github頁面回報。
感謝您的使用。

需求
----------------------
處理器：支援SSE指令集之x64處理器或支援Neon指令集之Arm64處理器。
系統：Windows 7 SP1以上作業系統。

**若您使用ARM64架構，請使用ARM64版本。 雖然ARM64版本Windows 11可開啟x64版本，但其程式標題列仍會顯示emulated X64字樣。**


版本說明
----------------------
### 25.12.31
#### 已實現
* 新增對Arm64架構支持（Arm64架構處理器需支援Neon指令集），Rho5將可利用Arm64的Neon指令集解密與加密。
* 現在可以利用「開啟」選項來開啟多個Rho與Jmd檔案。
* 修改名稱原則，使輸出檔案能作為DataRaw使用。
* KartLibrary與RaycityLibrary共通部分合併成KartCity.Common。
* 新增XML預覽功能，考量到Windows會預設使用瀏覽器開啟XML檔案，但是瀏覽器開啟XML的速度，可說是十分感人，所以增加此功能。
* 改善BML預覽效率，棄用RichTextBox以增加效率。
* 繁簡中文與韓文改以Noto Sans CJK字體。
* 改用非同步檔案載入方式，使檔案載入時不會沒有回應。
* 改善影像預覽功能。
* 新增快速預覽功能，不用雙擊即可在側邊預覽文件。
* RhoLoader與JmdLoader合併為KartCityLite。(意即您可以利用本工具開啟跑跑卡丁車與光速城市的遊戲檔案。)
#### 實現中
* 新增「賽道預覽」功能。
* 新增音樂預覽功能。
* 改檔功能，目前僅限KartLibrary實現Rho與Rho5改檔，KartStorageSystem正在實現中。


### 23.1.31
* 新增對Rho 1.0的支持。
* 新增對Rho5的支持。(僅在「開啟整個Data資料夾」時可用)
* 新增對韓語的支持。
* 新增「檢查更新」功能
* 新增「開啟整個Data資料夾」功能，您能藉此功能開啟Data資料夾內所有的Rho、Rho5檔案。
* 刪除些用不到的選項
* 修正一些錯誤
* 改善UI設計
* 改善KartRider File Library的資料結構。