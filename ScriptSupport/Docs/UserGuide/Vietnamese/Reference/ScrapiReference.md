# Scrapyard

## 1. Tổng quan

Scrapyard cung cấp tài liệu về các Functions, Constants, Namespaces, Enums, Tags và Types được định nghĩa bởi YGO OCG-Core.

Script Support tổ chức các thành phần này thành những danh mục có thể tìm kiếm, cho phép người dùng tra cứu tên, mô tả và tài liệu liên quan mà không cần phải tìm kiếm thủ công trong các tệp nguồn.

Dữ liệu Scrapi được hiển thị thông qua khu vực **Scrapi Books** trong Bảng Điều Hướng, trong khi tài liệu tương ứng được hiển thị tại khu vực **Scrapi Result**.

## 2. Cấu trúc Scrapi

Script Support tổ chức Scrapi thành sáu danh mục:

| Danh Mục    | Mô tả       |
|-------------|-------------|
| Functions   | Các Function được cung cấp bởi YGO OCG-Core. |
| Constants   | Các giá trị Constant được định nghĩa bởi YGO OCG-Core. |
| Namespaces  | Các Namespace được sử dụng để tổ chức những thành phần có liên quan. |
| Enums       | Các Enumeration và những Member tương ứng. |
| Tags        | Các Tag được định nghĩa bởi YGO OCG-Core. |
| Types       | Các Type được tham chiếu trong những định nghĩa Scrapi. |

Sáu danh mục này được gọi chung là **Scrapi** trong toàn bộ Hướng dẫn người dùng này.

## 3. Tìm Kiếm Scrapi

Khu vực Scrapi Books cung cấp hai Tab tìm kiếm: **Scr Name** và **Scr Desc**.

### 3.1. Tìm kiếm theo tên

Tab **Scr Name** cho phép người dùng tìm kiếm các thành phần Scrapi theo tên.

Nhập từ khóa tìm kiếm vào ô **Tìm tên Scrapi**. Danh sách kết quả sẽ hiển thị các Scrapi Entry phù hợp.

### 3.2. Tìm kiếm theo mô tả

Tab **Scr Desc** cho phép người dùng tìm kiếm các thành phần Scrapi dựa trên nội dung mô tả.

Nhập từ khóa tìm kiếm vào ô **Tìm mô tả Scrapi**. Danh sách kết quả sẽ hiển thị các Entry phù hợp dựa trên nội dung mô tả trong tài liệu tương ứng.

## 4. Xem thông tin Scrapi

Khi một Scrapi Entry được chọn từ một trong hai Tab tìm kiếm, tài liệu tương ứng sẽ được hiển thị tại khu vực **Scrapi Result**.

Khu vực này cung cấp các thông tin liên quan đến Entry được chọn, cho phép người dùng tra cứu tài liệu trong khi làm việc với Card Script.

## 5. Các tập lệnh thẻ liên quan

Script Support có thể hỗ trợ người dùng xác định các Card Script có sử dụng một thành phần Scrapi cụ thể.

### 5.1. Tự động tìm kiếm

Tùy chọn **Tự động tìm kiếm** tự động tìm kiếm các Card Script có chứa tên của Scrapi Entry đang được chọn.

Khi tùy chọn này được bật, việc chọn một Scrapi Entry sẽ kích hoạt quá trình tìm kiếm văn bản trên nội dung của các File `.lua` hiện có trong Data Source đã được cấu hình.

Quá trình tìm kiếm xác định các Script có nội dung văn bản chứa tên Scrapi được chọn. Những Script phù hợp sẽ được hiển thị trong khu vực Script Result, cho phép người dùng kiểm tra các ví dụ về cách thành phần Scrapi xuất hiện trong Card Script.

Tính năng này thực hiện tìm kiếm dựa trên văn bản. Nó không phân tích cú pháp Lua hoặc phân tích các biểu thức để xác định liệu một thành phần Scrapi có thực sự được gọi hoặc tham chiếu hay không.

Để bật hoặc tắt tính năng này, truy cập:

`Cài Đặt -> Cấu Hình -> Xử lý Dữ Liệu -> Tự động Tìm Kiếm`

Khi Tự động tìm kiếm bị tắt, việc chọn một Scrapi Entry sẽ không tự động kích hoạt quá trình tìm kiếm Card Script liên quan.

---

Tiếp theo: [Trình Soạn Thảo Mã →](../Reference/ScriptEditorReference.md)

Trước đó:

[← Thẻ Bài](../Reference/CardReference.md)

[← Hướng Dẫn Bắt Đầu](../GettingStarted.md)
