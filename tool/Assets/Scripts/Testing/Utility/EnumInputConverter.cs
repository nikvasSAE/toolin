using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


public class EnumInputConverter : StringEnumConverter
{
    public override void WriteJson(JsonWriter output, object value, JsonSerializer serializer){
        if (value == null){
            output.WriteNull();
        }
        else{
            Enum @enum = (Enum) value;
            string l_EnumToString = @enum.ToString("G");
            output.WriteRawValue(l_EnumToString);
        }
    }
}