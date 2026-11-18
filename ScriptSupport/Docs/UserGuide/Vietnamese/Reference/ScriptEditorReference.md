# Trình Soạn Thảo Mã

## 1. Tổng quan

Trình Soạn Thảo Mã cung cấp môi trường để xem và chỉnh sửa các Tập lệnh Lua được sử dụng bởi YGO OCG-Core.

Ngoài các chức năng chỉnh sửa văn bản cơ bản, Trình Soạn Thảo Mã còn cung cấp những tính năng hỗ trợ viết mã, giúp người dùng làm việc với Tập lệnh Lua và tra cứu tài liệu Scrapi ngay trong quá trình viết code.

Trình Soạn Thảo Mã bao gồm các tính năng sau:

- Tô sáng cú pháp Lua
- Tự động hoàn thành
- Mô tả Di chuột
- Quản lý Tab

## 2. Làm việc với các tệp tập lệnh

Script Support cho phép mở và chỉnh sửa các tệp Tập lệnh Lua ngay trong ứng dụng.

### 2.1. Mở các tệp tập lệnh

Người dùng có thể mở các tệp Tập lệnh hiện có thông qua lệnh **Mở** trong Menu.

Các tệp đã mở sẽ được liệt kê trong Tab **Mở** của Bảng điều hướng.

Khi chọn một Tệp từ danh sách này, nội dung của Tệp đó sẽ được hiển thị trong Trình Soạn Thảo Mã.

### 2.2. Tạo các tệp tập lệnh

Người dùng có thể tạo các tệp văn bản mới thông qua lệnh **Mới** trong Menu.

Các tệp mới tạo sẽ được liệt kê trong Tab **Mới** của Bảng điều hướng và có thể được chỉnh sửa trong Trình Soạn Thảo Mã.

### 2.3. Chỉnh sửa các tệp tập lệnh

Trình Soạn Thảo Mã cung cấp môi trường chỉnh sửa văn bản dành cho Tập lệnh Lua.

Người dùng có thể trực tiếp thay đổi nội dung Tập lệnh trong Trình soạn thảo. Tuy nhiên, để lưu các thay đổi vào Tệp gốc, tùy chọn **Cho phép Lưu** phải được bật.

Khi **Cho Phép Lưu** bị tắt, người dùng không thể lưu nội dung đã chỉnh sửa vào Tệp gốc.

Hạn chế này giúp bảo vệ các Tập lệnh Game hiện có khỏi những thay đổi ngoài ý muốn, đồng thời vẫn cho phép người dùng xem và làm việc với nội dung của chúng.

Để biết thêm thông tin, xem [Cấu hình](../Settings/Configuration.md).

## 3. Soạn thảo Mã

### 3.1. Tô sáng cú pháp

Trình Soạn Thảo Mã cung cấp tính năng Tô sáng cú pháp cho mã Lua.

Các thành phần cú pháp khác nhau được hiển thị bằng những kiểu định dạng riêng biệt, giúp tập lệnh dễ đọc và dễ hiểu hơn.

### 3.2. Tự Động Hoàn Thành

Cung cấp các gợi ý trong quá trình người dùng viết Tập lệnh Lua.

Hệ thống hỗ trợ nhiều danh mục Symbol, bao gồm:

- Functions
- Constants
- Enums và Enum Members
- Namespaces
- Tags
- Types
- Lua Keywords
- Variables

Người dùng có thể chủ động mở Completion List bằng phím tắt **Ctrl + Space**.

Completion List cũng có thể tự động xuất hiện trong quá trình nhập liệu.

Editor hỗ trợ Completion dựa trên ngữ cảnh sau toán tử truy cập Member (`.`), cho phép người dùng khám phá các Member hiện có của những Namespace hoặc Expression được nhận diện.

Nhấn **Esc** để đóng Completion List.

### 3.3. Mô tả Di chuột

Mô tả Di chuột cho phép người dùng tra cứu thông tin Scrapi trực tiếp từ Trình Soạn Thảo Mã.

Khi di chuyển con trỏ chuột đến một Symbol hoặc Expression được nhận diện, Editor sẽ hiển thị một Documentation Popup chứa thông tin tương ứng.

Điều này cho phép người dùng tra cứu tài liệu Scrapi mà không cần chuyển sang khu vực Scrapi Books.

## 4. Editor Tabs

Trình Soạn Thảo Mã sử dụng giao diện dựa trên Tab để quản lý nhiều tệp đang được mở.

### 4.1. Các Tab tạm thời và cố định

Editor hỗ trợ hai loại Tab:

**Tab Tạm thời**

- Xem nhanh nội dung của nhiều file mà không cần đóng Tab thủ công.
- Tiêu đề Tab luôn được đặt ở vị trí ngoài cùng bên phải của Tab Control.
- Tiêu đề Tab được hiển thị bằng kiểu chữ *in nghiêng*.
- Tại một thời điểm chỉ có thể tồn tại tối đa một Tab Tạm thời.
- Khi một Card Script khác được mở, Tab Tạm thời hiện tại sẽ được thay thế bằng Card Script mới được chọn.

**Tab Cố định**

- Tab sẽ được giữ lại để có thể xem nội dung lâu dài.
- Tiêu đề Tab được đặt về phía bên trái của Tab Control.
- Tiêu đề Tab được hiển thị bằng kiểu chữ in thẳng.
- Tại một thời điểm có thể tồn tại nhiều Tab Cố định.
- Tab tiếp tục được giữ mở khi người dùng chọn một Card Script khác.
- Nội dung của Tab không bị thay thế bởi Card Script mới được mở.
- Tiếp tục được giữ mở cho đến khi được đóng một cách chủ động.

Nếu hiện tại không có Tab Tạm thời nào đang mở, Item tiếp theo được mở bằng thao tác Single Click sẽ tạo một Tab Tạm thời mới.

Khi mở một Tab khác, Tab Tạm thời hiện tại sẽ bị thay thế nếu đang tồn tại. Các Tab Cố định không bị ảnh hưởng.

#### Chuyển Tab Tạm thời thành Tab Cố định

Tab Tạm thời có thể được chuyển thành Tab Cố định bằng một trong hai cách sau:

1. **Nhấn đúp vào Tiêu đề Tab.**
2. **Chỉnh sửa nội dung đang hiển thị.** Bất kỳ thay đổi nào đối với nội dung cũng sẽ tự động chuyển Temporary Tab thành Permanent Tab.

Sau khi được chuyển đổi, Tab sẽ tiếp tục được giữ mở và không còn bị thay thế khi người dùng chọn một Tập lệnh Thẻ khác.

### 4.2. Quản Lý Tab

Trình Soạn Thảo Mã hỗ trợ sắp xếp lại Tab thông qua các thao tác kéo và thả (Drag and Drop).

Người dùng cũng có thể tách Tab và gộp chúng vào các nhóm Tab khác, cho phép tổ chức bố cục Editor phù hợp với quy trình làm việc của mình.

---

Trước đó:

[← Scrapyard](../Reference/ScrapiReference.md)

[← Hướng Dẫn Bắt Đầu](../GettingStarted.md)
