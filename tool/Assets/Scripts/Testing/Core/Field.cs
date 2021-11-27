using System.Collections;
using System.Collections.Generic;

public class Field
{
    private int m_FieldFieldIndex;

    private string m_FieldName;
    private string m_FieldType;
    private List<int> m_FieldIndexes;
    private List<IntrospectionQueryField> m_AllFields;
    private bool m_HasChangedField;
    private bool m_HasSubFields;
            
    public int FieldIndex {
        get => m_FieldFieldIndex;
        set
        {
            FieldType = m_AllFields[value].type;
            FieldName = m_AllFields[value].name;
            if (value != m_FieldFieldIndex)
            {
                m_HasChangedField = true;
            }
            m_FieldFieldIndex = value;
        }
    }
    
    public string FieldName { get => m_FieldName; set => m_FieldName = value; }
    public string FieldType { get => m_FieldType; set => m_FieldType = value; }
    public List<int> ParentFieldIndexes { get => m_FieldIndexes; set => m_FieldIndexes = value; }
    public bool HasSubFields { get => m_HasSubFields; set => m_HasSubFields = value; }
    public List<IntrospectionQueryField> IntrospectionQueryFields { get => m_AllFields; internal set => m_AllFields = value; }
    public bool HasChangedField { get => m_HasChangedField; set => m_HasChangedField = value; }
            
    public Field()
    {
        m_AllFields = new List<IntrospectionQueryField>();
        m_FieldIndexes = new List<int>();
        m_FieldFieldIndex = 0;
    }
                
    public void CheckSubFields(Introspection.SchemaClass SchemaClass)
    {
        Introspection.SchemaClass.Data.Schema.Type l_SchemaType = SchemaClass
            .data
            .__schema
            .types
            .Find((schemaType => schemaType.name == FieldType));
                
        if (l_SchemaType.fields == null || l_SchemaType.fields.Count == 0){
            m_HasSubFields = false;
            return;
        }
        m_HasSubFields = true;
    }
                
    [System.Serializable]
    public class IntrospectionQueryField
    {
        public string name;
        public string type;
        public static implicit operator IntrospectionQueryField(Field field) => new IntrospectionQueryField{name = field.FieldName, type = field.FieldType};
    }
                
    public static explicit operator Field(Introspection.SchemaClass.Data.Schema.Type.Field schemaField)
    {
        Introspection.SchemaClass.Data.Schema.Type l_SchemaFieldType = schemaField.type;
        string l_TypeName;
        do
        {
            l_TypeName = l_SchemaFieldType.name;
            l_SchemaFieldType = l_SchemaFieldType.ofType;
        } while (l_SchemaFieldType != null);
        return new Field
        {
            FieldName = schemaField.name, FieldType = l_TypeName
        };
    }
}