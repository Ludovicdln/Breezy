using Breezy.Core.IO.Definitions.Properties;

namespace Breezy.Core.IO.Definitions.Extensions;

public static class PropertyDbDefExtensions
{
    private const string INT = "int";
    private const string LONG = "long";
    private const string SHORT = "short";
    private const string STRING = "string";
    private const string DATETIME = "DateTime";
    private const string FLOAT = "float";
    private const string BYTE = "byte";
    private const string CHAR = "char";
    private const string DOUBLE = "double";
    private const string BOOLEAN = "bool";
    private const string GUID = "Guid";
    private const string DECIMAL = "decimal";
    
    public static bool TryGetDbReader(this PropertyDbDefinition propertyDbDefinition, out string dbReader)
    {
        dbReader = "GetValue";
        
        switch (propertyDbDefinition.Type)
        {
            case INT:
                dbReader = "GetInt32";
                return true;
            
            case LONG:
                dbReader = "GetInt64";
                return true;
            
            case SHORT:
                dbReader = "GetInt16";
                return true;
            
            case BOOLEAN:
                dbReader = "GetBoolean";
                return true;
            
            case DECIMAL:
                dbReader = "GetDecimal";
                return true;
            
            case STRING:
                dbReader = "GetString";
                return true;
            
            case GUID:
                dbReader = "GetGuid";
                return true;
            
            case FLOAT:
                dbReader = "GetFloat";
                return true;
            
            case DOUBLE:
                dbReader = "GetDouble";
                return true;
            
            case BYTE:
                dbReader = "GetByte";
                return true;
            
            case CHAR:
                dbReader = "GetChar";
                return true;
            
            case DATETIME:
                dbReader = "GetDateTime";
                return true;
            
            default:
                return false;
        }
    }
}