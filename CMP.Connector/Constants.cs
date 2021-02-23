using Sitecore.Data;

namespace CMP.Connector
{
    public static class Constants
    {
        public static ID ImageFieldMappingTypeID = new ID("{40313FEB-8D8B-403D-BEB7-9ACB64302283}");
        public static ID FieldMappingSitecoreAssetIndexFieldID = new ID("{EBE8B49A-D744-4A66-9220-928B5B89A419}");
        public static ID FieldMappingSitecoreRenditionFieldID = new ID("{0DC3344C-F12E-47C4-A7CC-E9609DAFB25E}");
        public static ID DamImageTemplateID = new ID("{375DF460-3ECD-4C43-8BD4-A23A43290FE3}");
        public const string EntityIdMessagePropertyName = "target_id";
        public static readonly ID ConfigItemId = new ID("{F8843BF7-A31A-4F71-8900-ABD56B8FE34C}");
        public static readonly TemplateID EntityMappingTemplateId = new TemplateID(new ID("{E022206A-3482-46C2-8DDF-2F1C2C6C7B69}"));
        public static readonly ID EntityIdentifierFieldId = new ID("{9B0343A9-9F69-4E0F-A059-9215BC8FE422}");
        public static readonly ID ContentHubEntryTemplateId = new ID("{0D17BE5B-EC2A-4296-8D3F-930EB60DFE7C}");
        public static readonly ID EntityMappingContentTypeIdentifierFieldId = new ID("{9E009C2F-AD3C-4108-AA48-15689C1D4E1B}");
        public static readonly ID EntityMappingBucketFieldId = new ID("{3F883DCB-C341-4929-9216-D07F4F1B00BE}");
        public static readonly ID EntityMappingTemplateFieldId = new ID("{E9C95E69-0D3D-4AE3-B893-D1FF4BA36B03}");
        public static readonly ID FieldMappingTemplateId = new ID("{E43E2773-7A50-4145-A266-312024F27382}");
        public static readonly ID FieldMappingCmpFieldNameFieldId = new ID("{DA990633-7884-44A2-8638-94389C5CBFEC}");
        public static readonly ID FieldMappingSitecoreFieldNameFieldId = new ID("{DB99415A-FE86-4A2D-9583-DB92894CB80A}");
        public static readonly ID RelationFieldMappingTemplateId = new ID("{49652EB6-F98E-4114-B46A-925B9815578A}");
        public static readonly ID RelationFieldMappingCmpRelationFieldNameFieldId = new ID("{C3101AF2-0121-4011-A9E9-FB727BB40999}");
        public static readonly ID TagFieldMappingTemplateId = new ID("{AD17E15A-6B9B-4E55-8EBA-F3C2E2CD04C5}");
        public static readonly ID NonLeafNodeOptionId = new ID("{9D745E95-5958-4ADC-A243-435488C47CD5}");
        public static readonly ID TagNameOptionId = new ID("{49CE70D9-7517-432E-8B38-D444E84F4486}");
        public static readonly ID CustomTaxonomyProviderNameFieldId = new ID("{2CB726E6-F2A7-4ACD-9D37-89F91EA96B67}");
        public static readonly ID FullPathNameId = new ID("{E41250DB-373A-490A-903B-4522A164AB71}");
        public static readonly ID NodeNameId = new ID("{355B33DB-2E6A-4DD7-8E30-0CBBBCB954DB}");
        public static readonly ID EnableNonLeafNodeId = new ID("{123D26BF-0810-4C98-912D-3BEC5B5FD5E5}");
        public static readonly ID DisableNonLeafNodeId = new ID("{881F85E2-5317-450E-8D7E-41A43D8D5493}");
        public static readonly ID DefaultLanguage = new ID("{21CA0142-2D9E-46CE-99E6-12ADEEBCD635}");

        public static class PublicLink
        {
            public static class Properties
            {
                public const string RelativeUrl = "RelativeUrl";
                public const string VersionHash = "VersionHash";
                public const string ConversionConfiguration = "ConversionConfiguration";
            }
        }

        public static class Asset
        {
            public static class Properties
            {
                public const string Renditions = "Renditions";
                public const string FileName = "FileName";
                public const string Title = "Title";
            }
        }

        public static class DAMImageTemplate
        {
            public static class Fields
            {
                public const string VersionID = "Version ID";
                public const string MediaID = "Media ID";
                public const string Transform = "Transform";
                public const string Rendition = "Rendition";

                public const string IconField = "__Icon";
            }
        }

        
    }
}