namespace ExampleContracts.v1
{
    public class FilePermissionDto
    {
        public string FileName { get; set; }
        public Permissions OwnerPermissions { get; set; }
        public Permissions GroupPermissions { get; set; }
        public Permissions WorldPermissions { get; set; }
        public CustomClass Custom { get; set; }
        public DtoWithDictionary DtoWithDictionary { get; set; }
        public ComplexDictionary ComplexDictionary { get; set; }
    }
}
