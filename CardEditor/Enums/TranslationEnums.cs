namespace CardEditor.Enums
{
    public enum StatusConf
    {
        Unknown = 0,       // chưa tính, để bắt lỗi quên gán
        Translated,        // có trong VI, vế phải khác vế trái
        Untranslated,      // có trong VI, vế phải trùng vế trái
        New,               // EN mới có, VI không có
        SourceChanged,     // DEV sửa câu gốc, bản dịch cũ có thể lỗi thời
        Removed            // chỉ có trong VI (chỉ dùng nếu hiển thị danh sách dòng đã xóa)
    }
    public enum StringToken
    {
        system,
        victory,
        counter,
        setname,
    }
}
