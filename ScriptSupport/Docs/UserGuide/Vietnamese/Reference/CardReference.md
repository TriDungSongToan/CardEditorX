# Thẻ Bài

## 1. Tổng Quan

Card Reference cung cấp các công cụ tìm kiếm và tra cứu thông tin Thẻ từ Nguồn Dữ Liệu đã được cấu hình.

Tính năng này hỗ trợ hai phương thức tìm kiếm chính:

- **Tìm kiếm Mô tả Thẻ:** Tìm kiếm các bản ghi Card Text dựa trên Tên, Mô tả, Chuỗi.
- **Tìm kiếm Tập lệnh Thẻ:** Tìm kiếm nội dung bên trong cáctệp Tập lệnh Lua.

Ngoài ra, người dùng có thể sử dụng Tìm Kiếm Nâng Cao để tìm Thẻ dựa trên thông tin văn bản và các thuộc tính của Thẻ, sau đó xem Mô tả Thẻ và các tệp Tập Lệnh tương ứng.

## 2. Cơ chế Tải dữ liệu Thẻ

Trước khi sử dụng Card Reference, người dùng nên tìm hiểu cách Script Support tải và tổ chức dữ liệu Card từ Nguồn Dữ Liệu.

### 2.1. Nguồn Dữ Liệu

Một Nguồn Dữ Liệu có thể chứa nhiều tệp Cơ Sở Dữ Liệu và nhiều tệp Tập lệnh.

Trong quá trình tải dữ liệu, Script Support đọc các tệp Cơ sở Dữ Liệu hiện có để lấy thông tin Thẻ, đồng thời xây dựng cấu trúc tra cứu cho các tệp Tập lệnh.

Quá trình tải tạo ra hai nhóm dữ liệu chính:

- **Thông tin Thẻ:** Bao gồm Dữ liệu Thẻ và Văn bản Thẻ được Load từ các tệp Cơ sở Dữ Liệu.
- **Tập lệnh Thẻ:** Cấu trúc tra cứu liên kết ID Thẻ với đường dẫn của các Script File tương ứng.

### 2.2. Cấu trúc Cơ Sở Dữ Liệ YGO

Cơ Sở Dữ Liệu YGO tiêu chuẩn sử dụng hai bảng chính để lưu trữ thông tin Thẻ:

| Bảng    | Mô tả |
|---------|-------------|
| `datas` | Lưu trữ Card Data, bao gồm `scope`, `type`, `race`, `attribute` và các thuộc tính khác. |
| `texts` | Lưu trữ Card Text, bao gồm `name`, `desc` và `str1` ~ `str16`. |

Hai bảng được liên kết với nhau thông qua cột `id`, đại diện cho ID Thẻ.

### 2.3. Cơ chế tải mặc định của YGO Game Engine

Theo mặc định, YGO Game Engine sử dụng cơ chế tải theo kiểu ghi đè (Overwrite).

Khi nhiều bản ghi Thẻ có cùng ID được tải, bản ghi được tải sau sẽ thay thế thông tin Thẻ đã được tải trước đó.

Do đó, chỉ thông tin của phiên bản được tải cuối cùng được giữ lại và có thể truy xuất.

### 2.4. Logic tải dữ liệu của Script Support

Script Support không sử dụng cơ chế ghi đè mặc định trên. Thay vào đó, Dữ liệu Thẻ và Văn bản Thẻ được xử lý độc lập với nhau.

**Dữ liệu Thẻ**

- Mỗi ID Thẻ chỉ lưu giữ một bản ghi Dữ liệu Thẻ.
- Khi nhiều bản ghi Dữ liệu có cùng ID Thẻ, bản ghi được tải đầu tiên sẽ được giữ lại.
- Các bản ghi tiếp theo có cùng ID sẽ bị bỏ qua và không ghi đè lên dữ liệu hiện có.

**Văn bản Thẻ**

- Mỗi ID Thẻ quản lý một `List<CardText>` chứa các bản ghi Text đã được Load.
- Mỗi bản ghi Văn bản được đọc từ Cơ sở dữ liệu đều được thêm vào danh sách tương ứng.
- Không thực hiện lọc hoặc loại bỏ bản ghi trùng lặp dựa trên ngôn ngữ, nội dung văn bản hay bất kỳ thuộc tính nào khác.

Do đó, nhiều bản ghi Văn bản có thể cùng tồn tại dưới một ID Thẻ, kể cả khi chúng có cùng ngôn ngữ hoặc nội dung giống hệt nhau.

Cách tiếp cận này cho phép Script Support giữ lại toàn bộ Văn bản Thẻ hiện có, đồng thời vẫn duy trì một bản ghi Dữ liệu Thẻ duy nhất cho mỗi ID.

### 2.5. Tải Tập lệnh Thẻ

Tập lệnh Thẻ được lưu trữ dưới dạng các tệp Lua văn bản thuần (Plain-text) riêng biệt.

Quy ước đặt tên Tệp tiêu chuẩn là:

```text
c{id}.lua
```

Ví dụ:

```text
c1001.lua
c1002.lua
c1003.lua
```

Trong quá trình tải, Script Support xây dựng cấu trúc tra cứu sau:

```csharp
Dictionary<ulong, List<string>> CardScripts;
```

Từ điển sử dụng:

- **Key (`ulong`):** Card ID.
- **Value (`List<string>`):** Danh sách đường dẫn tệp tương ứng với ID Thẻ đó.

Vì vậy, một ID Thẻ có thể liên kết với nhiều Tệp Tập lệnh và toàn bộ các đường dẫn tương ứng đều được giữ lại trong danh sách.

Cấu trúc này cho phép Script Support tìm kiếm và hiển thị tất cả tệp Tập lệnh liên quan đến Thẻ được chọn.

## 3. Tìm kiếm Thẻ

Card Reference cung cấp hai phương thức tìm kiếm: `Tìm kiếm mô tả Thẻ` và `Tìm kiếm Tập lệnh Thẻ`.

### 3.1. Tìm kiếm mô tả Thẻ

Ô tìm kiếm `Tìm kiếm Thẻ` cho phép người dùng tìm Thẻ dựa trên thông tin văn bản.

Quá trình tìm kiếm được thực hiện trên toàn bộ các bản ghi Văn bản Thẻ đã được tải của tất cả Thẻ trong Nguồn dữ liệu.

Các trường được sử dụng để tìm kiếm bao gồm:

| Trường | Mô tả |
|--------|-------|
| `Name` | Tên Card. |
| `Desc` | Nội dung mô tả và hiệu ứng của Card. |
| `Str`  | Các nội dung văn bản bổ sung của Card. |

Vì mỗi ID Thẻ có thể chứa nhiều bản ghi Văn bản Thẻ, quá trình tìm kiếm sẽ kiểm tra tất cả bản ghi đã được tải trong `List<CardText>` của từng Thẻ.

Điều này có nghĩa là một Thẻ có thể được tìm thấy thông qua bất kỳ bản ghi Văn bản nào của nó, bất kể ngôn ngữ hoặc việc nội dung có trùng lặp với bản ghi khác hay không.

### 3.2. Tìm kiếm Tập lệnh Thẻ

Ô tìm kiếm `Tìm kiếm Tập Lệnh` cho phép người dùng tìm Thẻ dựa trên nội dung của tệp Tập lệnh.

Khác với `Tìm kiếm Thẻ`, `Tìm kiếm Tập Lệnh` không tìm kiếm trong một trường Script của Cơ sở dữ liệu. Thay vào đó, phương thức này tìm kiếm chuỗi văn bản được chỉ định bên trong nội dung của các tệp Tập lện Lua.

Quá trình tìm kiếm được thực hiện trên toàn bộ các đường dẫn tệp Tập lệnh hiện có trong Nguồn dữ liệu.

Vì mỗi tệp Tập lệnh là một tệpe văn bản thuần riêng biệt, hệ thống sẽ kiểm tra nội dung của từng tệp để xác định các kết quả phù hợp.

### 3.3. Kết quả tìm kiếm

Cả hai phương thức tìm kiếm đều hiển thị các Thẻ phù hợp trong danh sách kết quả tương ứng.

Người dùng có thể mở một kết quả bằng các thao tác chuột sau:

| Tương tác bằng chuột | Hành vi         |
|----------------------|-----------------|
| Nhấp đơn chuột trái  | Mở nội dung được chọn trong Tab tạm thời. |
| Nhấp đôi chuột trái  | Mở nội dung được chọn trong Tab cố định. |

Đối với Tìm kiếm Mô tả Thẻ, khi chọn một Card, hệ thống sẽ hiển thị Mô tả Thẻ hiện có của Thẻ đó.

Đối với Tìm kiếm Tập lệnh Thẻ, khi chọn một kết quả, hệ thống sẽ mở nội dung Tập lệnh tương ứng.

## 4. Tìm kiếm Nâng cao

Tìm kiếm Nâng cao cung cấp các tiêu chí bổ sung để tìm kiếm Card.

Khu vực này được chia thành ba phần: Văn bản Thẻ, Dữ liệu Thẻ và Thông tin Thẻ.

### 4.1. Văn bản Thẻ

Văn bản Thẻ cho phép người dùng tìm kiếm Card dựa trên thông tin văn bản.

Các trường tìm kiếm hiện có bao gồm:

| Trường     | Mô tả           |
|------------|-----------------|
| ID Thẻ     | Tìm kiếm theo Card ID. |
| Bí danh Thẻ| Tìm kiếm theo Bí danh Thẻ. |
| Tên Thẻ    | Tìm kiếm theo tên Card. |
| Mô tả Thẻ  | Tìm kiếm trong nội dung mô tả Card. |
| Chuỗi      | Tìm kiếm trong các nội dung văn bản bổ sung của Card. |

Các trường này cung cấp những tùy chọn tìm kiếm cụ thể hơn so với phương thức Tìm kiếm Mô tả Thẻ thông thường.

### 4.2. Dữ liệu Thẻ

Dữ liệu Thẻ cho phép người dùng tìm kiếm Thẻ dựa trên các thuộc tính Thẻ.

Các thuộc tính tìm kiếm hiện có bao gồm:

- Phạm vi
- Loại
- Chủng tộc
- Thuộc tính
- Các thuộc tính khác được hỗ trợ.

Phần này hữu ích khi người dùng muốn tìm Thẻ dựa trên đặc điểm và thuộc tính thay vì nội dung văn bản.

### 4.3. Thông tin Thẻ

Thông tin Thẻ hiển thị thông tin chi tiết của Thẻ đang được chọn.

Tùy thuộc vào dữ liệu hiện có, phần này có thể bao gồm:

- Tên Thẻ
- Loại Thẻ
- Hình ảnh Thẻ
- Các thông tin khác của Thẻ.

Phần này được thiết kế để tra cứu thông tin Thẻ thay vì thiết lập các tiêu chí tìm kiếm.

## 5. Xem thông tin Thẻ

Sau khi một Thẻ được chọn, Script Support sẽ hiển thị những thông tin hiện có của Card đó trong **Result Panel**.

### 5.1. Mô tả Thẻ

Mô tả Thẻ được hiển thị trong Tab `Desc Result`.

Nội dung hiển thị tương ứng với một bản ghi Văn bản Thẻ được chọn thuộc ID Thẻ hiện tại.

Nếu cùng một ID Thẻ có nhiều bản ghi Văn bản Thẻ, người dùng có thể lựa chọn bản ghi muốn hiển thị.

### 5.2. Các phiên bản Mô tả Thẻ

Một ID Thẻ có thể có nhiều bản ghi Văn bản Thẻ vì Script Support giữ lại toàn bộ các bản ghi Văn bản đã được tải, thay vì ghi đè hoặc lọc bỏ chúng.

Một Hộp kết hợp nằm phía trên khu vực hiển thị Mô tả cho phép người dùng lựa chọn bản ghi Văn bản Thẻ muốn xem.

Việc lựa chọn dựa trên các bản ghi hiện có trong dữ liệu đã tải. Script Support không tự động nhận diện, phân loại hoặc loại bỏ các bản ghi dựa trên ngôn ngữ hay nội dung trùng lặp.

### 5.3. Tập lệnh Thẻ

Khi một Card được chọn, Script Support sử dụng Card ID của Card đó để tra cứu các đường dẫn Script File tương ứng trong `CardScripts`.

<!--

Thao tác tra cứu về mặt logic tương đương với đoạn code sau:

```csharp
if (CardScripts.TryGetValue(cardId, out var paths))
{
    // Display the corresponding Script Files.
}
```
-->

Mỗi đường dẫn đại diện cho một Script File riêng biệt.

Tất cả Script File tương ứng sẽ được hiển thị trong `Script Result`, mỗi File được mở trong một TabItem riêng.

Do đó:

- Nếu một ID Thẻ có một tệp Tập lệnh tương ứng, một Tab sẽ được hiển thị.
- Nếu một ID Thẻ có nhiều tệp Tập lệnh tương ứng, nhiều Tab sẽ được hiển thị.
- Nếu không có tệp Tập lệnh nào liên kết với ID Thẻ, sẽ không có nội dung Tập lệnh tương ứng để hiển thị.

---

Tiếp theo: [Scrapyard →](../Reference/ScrapiReference.md)

Trước đó:

[← Tổng quan Giao diện](../Interface/InterfaceOverview.md)

[← Hướng Dẫn Bắt Đầu](../GettingStarted.md)
