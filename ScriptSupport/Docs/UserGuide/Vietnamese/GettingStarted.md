# Hướng Dẫn Bắt Đầu

Hướng dẫn này sẽ giúp bạn thiết lập Script Support lần đầu tiên và thực hiện một quy trình làm việc cơ bản với Thư Viện Tra Cứu Dữ Liệu Thẻ.

Nếu bạn đang tìm kiếm hướng dẫn xây dựng Dự Án, hãy xem [README](../../../README.md).

## Các Điều Kiện Tiên Quyết

Trước khi bắt đầu, hãy đảm bảo rằng bạn có:

- **Hệ Điều Hành:** Windows 7, 10 hoặc 11
- **Môi Trường Chạy:** .NET 8
- **Kết nối Internet:** Cần thiết để tải xuống các gói dữ liệu trong quá trình thiết lập ban đầu.

## Tải Xuống Gói Dữ Liệu

Khi Script Support được khởi chạy lần đầu tiên, ứng dụng sẽ cần tải xuống và khởi tạo các gói dữ liệu cần thiết.

Hãy đảm bảo rằng bạn có kết nối Internet đang hoạt động và chờ quá trình khởi tạo hoàn tất trước khi sử dụng Ứng dụng.

Không đóng Ứng Dụng trong khi quá trình tải xuống gói dữ liệu ban đầu đang diễn ra.

Sau khi các gói cần thiết được cài đặt, Script Support đã sẵn sàng để cấu hình.

---

## I. Thiết Lập Ban Đầu

Sau khi các gói dữ liệu cần thiết được tải xuống, mở **Cài Đặt** → **Cấu Hình** để hoàn tất thiết lập ban đầu.

Hãy hoàn tất cấu hình Ứng dụng trước khi sử dụng các tính năng hiện có.

Để xem mô tả chi tiết về tất cả các thiết lập hiện có, hãy xem [Cấu Hình](./Settings/Configuration.md).

## II. Tổng quan Giao Diện

Script Support cung cấp các Panel để duyệt thông tin Thẻ, khám phá tài liệu Scrapi và làm việc với Tập lệnh Lua.

Trước khi tiếp tục, hãy làm quen với giao diện chính của Ứng dụng.

Để xem giải thích chi tiết về từng Panel và chức năng của chúng, hãy xem [Tổng quan Giao Diện](./Interface/InterfaceOverview.md).

## III. Bước đầu tiên

Sau khi hoàn tất thiết lập ban đầu, bạn có thể bắt đầu khám phá các tính năng chính của Script Support.

### 1. Tìm Kiếm Thẻ

Bạn có thể tìm kiếm Thẻ thông qua Mô tả hoặc các tập lệnh Lua liên quan.

Để tìm kiếm Thẻ bằng Mô tả:

1. Mở tab **Thẻ** trong Bảng điều hướng.
2. Nhập từ khóa tìm kiếm vào ô **Tìm kiếm Thẻ**.
3. Chọn một kết quả phù hợp để xem Mô tả Thẻ tương ứng.

Mô tả của Thẻ được chọn sẽ hiển thị trong **Desc Result**. Nếu có nhiều Văn bản Thẻ thuộc cùng một ID Thẻ, bạn có thể sử dụng ComboBox để chuyển đổi giữa chúng.

Để tìm kiếm Tập lệnh THẻ:

1. Mở tab **Tập Lệnh** trong Bảng điều hướng.
2. Nhập từ khóa tìm kiếm vào ô **Tìm kiếm Tập Lệnh**.
3. Chọn một kết quả phù hợp để mở Tập lệnh tương ứng.

Bạn cũng có thể chọn một Thẻ để xem các Tập lệnh Thẻ liên quan trong **Script Result**, nếu có.

Để biết thêm thông tin về Dữ Liệu Thẻ, Văn Bản Thẻ, Tập Lệnh Thẻ và các tùy chọn tìm kiếm, hãy xem [Thẻ Bài](./Reference/CardReference.md).

### 2. Khám Phá Scrapyard

Scrapi Books cho phép bạn tìm kiếm và xem các thành phần được định nghĩa bởi YGO OCG-Core.

**Để tìm một Scrapi Entry theo tên:**

1. Mở tab **Scr Name** trong **Scrapi Books**.
2. Nhập từ khóa tìm kiếm vào ô **Tìm tên Scrapi**.
3. Chọn một kết quả phù hợp để xem tài liệu tương ứng trong **Scrapi Result**.

Bạn cũng có thể sử dụng tab Scr Desc để tìm kiếm Scrapi Entry thông qua Mô tả của chúng.

Nếu **Tự Động TÌm Kiếm** được bật, việc chọn một Scrapi Entry sẽ tự động tìm kiếm tên của nó trong các Tập lệnh Lua hiện có. Những Tập lệnh phù hợp sẽ được hiển thị trong **Script Result**.

Để biết thêm thông tin, hãy xem [Scrapyard](./Reference/ScrapiReference.md).

### 3. Mở và chỉnh sửa Tập Lệnh

Script Support cho phép bạn mở và chỉnh sửa trực tiếp các tệp tập lệnh Lua trong Script Editor.

Để mở một Tập lệnh hiện có:

1. Mở Menu.
2. Chọn Mở và chọn Tệp Tập lệnh Lua cần mở.
3. Tệp được mở sẽ xuất hiện trong tab Open của Bảng điều hướng.
4. Chọn Tệp để hiển thị nội dung của nó trong Script Editor.

### Quyền xử lý dữ liệu

Script Support chủ yếu được thiết kế như một công cụ tham khảo và tra cứu dữ liệu cũng như Tập lệnh của Yu-Gi-Oh! OCG-Core.

Để giúp bảo vệ các Tệp tập lệnh gốc khỏi những thay đổi ngoài ý muốn, Script Support cung cấp hai tùy chọn độc lập tại:

```text
Cài Đặt → Cấu Hình → Xử Lý Dữ Liệu
```

- **Cho Phép Lưu**: Cho phép người dùng lưu nội dung hiện đang hiển thị trong Script Editor trở lại File gốc.

- **Cho Phép Tạo Mới**: Cho phép người dùng tạo các Text File mới, bao gồm cả Lua Script File.

Cả hai tùy chọn có thể được bật hoặc tắt độc lập.

Nếu bạn chỉ có ý định sử dụng Script Support để duyệt và tham khảo dữ liệu Game, bạn có thể để cả hai tùy chọn ở trạng thái Tắt.

Điều này giúp ngăn chặn những thay đổi ngoài ý muốn đối với các Game Script hiện có và tránh việc vô tình tạo thêm tệp mới.

Để biết thêm thông tin về chỉnh sửa Tập lệnh Thẻ và quản lý các Editor Tab, hãy xem [Trình Soạn Thảo Mã](./Reference/ScriptEditorReference.md).

---
