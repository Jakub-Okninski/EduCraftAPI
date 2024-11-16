namespace EduCraftAPI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlRoot("Presentation")]
    public class Presentation
    {
        public int PresentationID { get; set; }
        public string Title { get; set; }
        [XmlArray("Slides")]
        [XmlArrayItem("Slide")]
        public List<Slide>? Slides { get; set; }
    }
    public class Slide
    {
        [XmlAttribute("Id")]
        public int Id { get; set; }
        public string? Title { get; set; }
        [XmlArray("Elements")]
        [XmlArrayItem("Element")]
        public List<Element>? Elements { get; set; }
    }

    public class Element
    {
        [XmlAttribute("Id")]
        public int Id { get; set; }
        public string? Type { get; set; }
        public Position? Position { get; set; } = new Position();
        public Size? Size { get; set; } = new Size();
        [XmlArray("Ops")]
        [XmlArrayItem("Op")]
        public List<Op>? Ops { get; set; }
        public string? PathName { get; set; }
        public string? Url { get; set; }

        public override string ToString()
        {
            var properties = new List<string>();
            properties.Add($"Type: {Type}");
            properties.Add($"Position: {(Position != null ? Position.ToString() : "null")}");
            properties.Add($"Size: {(Size != null ? Size.ToString() : "null")}");
            if (Ops != null && Ops.Count > 0)
            {
                properties.Add("Ops: " + string.Join(", ", Ops));
            }
            else
            {
                properties.Add("Ops: null or empty");
            }
            properties.Add($"Url: {Url}");
            return string.Join(", ", properties);
        }
    }

    public class Position
    {
        public float? Top { get; set; } = null;
        public float? Left { get; set; } = null;
    }
    public class Size
    {
        public float? Width { get; set; } = null;
        public float? Height { get; set; } = null;
    }
    public class Op
    {
        [XmlIgnore]
        public string? Insert
        {
            get => _insert;
            set
            {
                _insert = value;
                _insertCode = value?.Replace("\n", "&#10;");
            }
        }

        private string? _insert;
        private string? _insertCode;
        [XmlElement("Insert")]
        public string? InsertDecode
        {
            get => _insertCode;
            set
            {
                _insertCode = value;
                _insert = value?.Replace("&#10;", "\n");
            }
        }
        public Attributes? Attributes { get; set; } = null;
        public override string ToString()
        {
            if (Attributes == null)
            {
                return $"Insert: {Insert}";
            }
            return $"Insert: {Insert}, Attributes: {Attributes}";
        }
    }
    public class Attributes
    {
        public bool? Bold { get; set; }
        public bool? Italic { get; set; }
        public bool? Underline { get; set; }
        public bool? Strike { get; set; }
        public bool? Blockquote { get; set; }
        public int? Header { get; set; }
        public string? Script { get; set; }
        public int? Indent { get; set; }
        public string? Align { get; set; }
        public string? Direction { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? Background { get; set; }
        public string? Font { get; set; }
        public string? Link { get; set; }
        public string? List { get; set; }
        public override string ToString()
        {
            var properties = new List<string>();
            if (Bold.HasValue) properties.Add($"Bold: {Bold.Value}");
            if (Italic.HasValue) properties.Add($"Italic: {Italic.Value}");
            if (Underline.HasValue) properties.Add($"Underline: {Underline.Value}");
            if (Strike.HasValue) properties.Add($"Strike: {Strike.Value}");
            if (Blockquote.HasValue) properties.Add($"Blockquote: {Blockquote.Value}");
            if (Header.HasValue) properties.Add($"Header: {Header.Value}");
            if (Script != null) properties.Add($"Script: {Script}");
            if (Indent.HasValue) properties.Add($"Indent: {Indent.Value}");
            if (Align != null) properties.Add($"Align: {Align}");
            if (Direction != null) properties.Add($"Direction: {Direction}");
            if (Size != null) properties.Add($"Size: {Size}");
            if (Color != null) properties.Add($"Color: {Color}");
            if (Background != null) properties.Add($"Background: {Background}");
            if (Font != null) properties.Add($"Font: {Font}");
            if (Link != null) properties.Add($"Link: {Link}");
            if (List != null) properties.Add($"List: {List}");
            return string.Join(", ", properties);
        }
    }
}
