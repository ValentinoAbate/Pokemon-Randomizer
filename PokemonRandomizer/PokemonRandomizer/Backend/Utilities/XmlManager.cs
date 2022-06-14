using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PokemonRandomizer.Backend.Utilities
{
    public class XmlManager
    {
        public const string offsetAttr = "offset";
        public const string pointerAttr = "pointer";
        public const string pointerPrefixAttr = "pointerPrefix";
        public const string prefixAttr = "prefix";
        public const string signatureAttr = "signature";
        public const string numAttr = "num";
        public const string sizeAttr = "size";
        public const string lengthAttr = "length";
        public const string paddingAttr = "padding";
        public const string pathAttr = "filePath";
        public const string constantsElt = "constants";
        public const string inheritanceElt = "inheritFrom";

        private const char arrayEnd = ']';
        private const char arrayStart = '[';
        private const char arraySeparator = ',';

        public XElement Root { get; }
        private XElement searchRoot;

        private readonly Dictionary<string, XElement> cache = new Dictionary<string, XElement>();

        #region Constructors
        public XmlManager(string doc) : this(XDocument.Parse(doc).Root) { }
        public XmlManager(XElement root)
        {
            Root = root;
            searchRoot = root;
            // Inheritance
            foreach(var elt in root.Elements())
            { 
                // Check if we have a valid inheritance and base element
                if (!elt.HasElements)
                    continue;
                var firstElt = elt.Elements().First();
                if (firstElt.Name.LocalName != inheritanceElt)
                    continue;
                var superElt = Element(firstElt.Value, false);
                if (superElt == null)
                    continue;
                // Do actual inheritance
                searchRoot = elt;
                foreach (var inheritedElt in superElt.Descendants())
                {
                    // Does the inheriting element have an element with the same name?
                    var overrideElt = Element(inheritedElt.Name.LocalName, false);
                    if (overrideElt == null) // If not, add a clone of the inheritedElt
                    {
                        elt.Add(new XElement(inheritedElt) as XNode);
                    }
                    else // Add the attributes that are not overriden by the inhering node
                    {
                        foreach (var attr in inheritedElt.Attributes())
                        {
                            // Only add values that don't have values on the override elt
                            if (overrideElt.Attribute(attr.Name.LocalName) == null)
                            {
                                overrideElt.SetAttributeValue(attr.Name.LocalName, attr.Value);
                            }
                        }
                    }
                }
                searchRoot = root;
            }
        }
        #endregion

        /// <summary> Sets the element (by name) to consider as the root when searching
        /// If the name is not in the cache it is searched for (without caching) </summary>
        public void SetSearchRoot(string element)
        {
            searchRoot = Root;
            searchRoot = Element(element, false);
        }
        /// <summary> Sets the element to consider as the root when searching</summary>
        public void SetSearchRoot(XElement element)
        {
            searchRoot = element;
        }
        /// <summary> Clears the cache dictionary</summary>
        public void ClearCache()
        {
            cache.Clear();
        }

        #region Utility/Special Attribute functions/Element Conversion functions
        /// <summary> Converts a hex string to an integer </summary> 
        public static Int32 HexToInt(string hex)
        {
            return Convert.ToInt32(hex, 16);
        }
        public static byte HexToByte(string hex) => Convert.ToByte(hex, 16);
        /// <summary>
        /// Finds the offset of the specified data using various methods.
        /// If no method successfully finds a valid offset, return Rom.nullPointer.
        /// If "isAtValidOffset" == null, any offset in the rom is considered valid.
        /// TODO: Check that the thing at the "pointer" location is actually a pointer
        /// TODO: Check that the thing after the prefix in find pointer prefix is actually a pointer
        /// </summary>
        public int FindOffset(string element, Rom rom, Func<Rom, int, bool> isValidOffset = null)
        {
            if(HasPointerPrefix(element))
            {
                try
                {
                    int offset = rom.ReadPointer(rom.FindFromPrefix(Attr(element, pointerPrefixAttr)));
                    if(rom.IsValidOffset(offset))
                    {
                        if (isValidOffset == null || isValidOffset(rom, offset))
                            return offset;
                    }
                }
                catch { }
            }
            if (HasPrefix(element))
            {
                try
                {
                    int offset = rom.FindFromPrefix(Attr(element, prefixAttr));
                    if (rom.IsValidOffset(offset))
                    {
                        if (isValidOffset == null || isValidOffset(rom, offset))
                            return offset;
                    }
                }
                catch { }
            }
            if (HasSignature(element))
            {
                try
                {
                    int offset = rom.FindFirst(Attr(element, signatureAttr));
                    if (rom.IsValidOffset(offset))
                    {
                        if (isValidOffset == null || isValidOffset(rom, offset))
                            return offset;
                    }
                }
                catch { }
            }
            if (HasPointer(element))
            {
                int offset = rom.ReadPointer(Pointer(element));
                if(rom.IsValidOffset(offset))
                {
                    if (isValidOffset == null || isValidOffset(rom, offset))
                        return offset;
                }
            }
            if(HasOffset(element))
            {
                int offset = Offset(element);
                if (rom.IsValidOffset(offset))
                {
                    if (isValidOffset == null || isValidOffset(rom, offset))
                        return offset;
                }
            }
            return Rom.nullPointer;
        }
        /// <summary>
        /// Set the Rom's internal offset to the offset of the specified data (found using FindOffset).
        /// Returns false if no valid offset is found
        /// </summary>
        public bool FindAndSeekOffset(string element, Rom rom, Func<Rom, int, bool> isValidOffset = null)
        {
            int offset = FindOffset(element, rom, isValidOffset);
            if(offset != Rom.nullPointer)
            {
                rom.Seek(offset);
                return true;
            }
            return false;
        }
        public string Path(string element)
        {
            return Attr(element, pathAttr);
        }
        /// <summary> returns the "num" (amount of entries) attribute of the given element. Expects an integer value </summary>
        public int Num(string element)
        {
            return IntAttr(element, numAttr);
        }
        /// <summary> returns the "size" (size in bytes) attribute of the given element. Expects an integer value </summary>
        public int Size(string element)
        {
            return IntAttr(element, sizeAttr);
        }
        /// <summary> returns the "length" (max string length) attribute of the given element. Expects an integer value </summary>
        public int Length(string element)
        {
            return IntAttr(element, lengthAttr);
        }
        public int Padding(string element)
        {
            return IntAttr(element, paddingAttr);
        }
        /// <summary> returns true if the element has an "offset" (offset) attribute </summary>
        public bool HasOffset(string element)
        {
            return HasElementWithAttr(element, offsetAttr);
        }
        /// <summary> returns the "offset" (offset) attribute of the element converted from hex string to int </summary>
        public int Offset(string element)
        {
            return HexAttr(element, offsetAttr);
        }
        /// <summary> returns true if the element has a "pointer" (pointer) attribute </summary>
        public bool HasPointer(string element)
        {
            return HasElementWithAttr(element, pointerAttr);
        }
        /// <summary> returns the "pointer" (pointer) attribute of the element converted from hex string to int </summary>
        public int Pointer(string element)
        {
            return HexAttr(element, pointerAttr);
        }
        /// <summary> returns true if the element has a "pointerPrefix" (pointer prefix) attribute </summary>
        public bool HasPointerPrefix(string element)
        {
            return HasElementWithAttr(element, pointerPrefixAttr);
        }
        /// <summary> returns true if the element has a "pointerPrefix" (pointer prefix) attribute </summary>
        public bool HasPrefix(string element)
        {
            return HasElementWithAttr(element, prefixAttr);
        }
        /// <summary> returns true if the element has a "pointerPrefix" (pointer prefix) attribute </summary>
        public bool HasSignature(string element)
        {
            return HasElementWithAttr(element, signatureAttr);
        }

        /// <summary> returns the given attribute of the element converted from hex string to int </summary> 
        public int HexAttr(string element, string attribute)
        {
            return HexToInt(Attr(element, attribute));
        }
        /// <summary> returns the given attribute of the element converted from string to int </summary> 
        public int IntAttr(string element, string attribute)
        {
            return int.Parse(Attr(element, attribute));
        }
        /// <summary> returns the given attribute of the element unpacked from an array </summary>
        public string[] ArrayAttr(string element, string attribute)
        {
            var baseArray = Attr(element, attribute);
            if(string.IsNullOrWhiteSpace(baseArray))
                return Array.Empty<string>();
            return baseArray.Trim(arrayStart, arrayEnd).Split(arraySeparator);
        }
        public string[] ArrayAttrLowerCase(string element, string attribute)
        {
            var baseArray = Attr(element, attribute);
            if (string.IsNullOrWhiteSpace(baseArray))
                return Array.Empty<string>();
            return baseArray.ToLower().Trim(arrayStart, arrayEnd).Split(arraySeparator);
        }
        /// <summary> returns the given attribute of the element with each element interpreted as an int[] </summary>
        public int[] IntArrayAttr(string element, string attribute)
        {
            return Array.ConvertAll(ArrayAttr(element, attribute), int.Parse);
        }
        /// <summary> returns the given attribute of the element with each element interpreted from a hex string </summary>
        public int[] HexArrayAttr(string element, string attribute)
        {
            return Array.ConvertAll(ArrayAttr(element, attribute), HexToInt);
        }

        public byte[] ByteArrayAttr(string element, string attribute)
        {
            return Array.ConvertAll(ArrayAttr(element, attribute), HexToByte);
        }

        public PokemonType[] TypeArrayAttr(string element, string attribute)
        {
            return Array.ConvertAll(ArrayAttrLowerCase(element, attribute), PokemonTypeUtils.FromString);
        }

        /// <summary> returns the given element's content as a string </summary> 
        public string StringElt(string element)
        {
            return (string)Element(element);
        }
        #endregion
        /// <summary>
        /// Returns true if the given element exists and has the given attribute.
        /// If the element is cached, it is looked up,
        /// else it is searched for (with SearchRoot as the search root Node)
        /// </summary>
        public bool HasElementWithAttr(string element, string attribute)
        {
            return Attr(element, attribute) != null;
        }

        public string Attr(string element, string attribute)
        {
            return Element(element)?.Attribute(attribute)?.Value;
        }

        public string AttrLowerCase(string element, string attribute)
        {
            return Element(element)?.Attribute(attribute)?.Value.ToLower();
        }

        /// <summary>
        /// Returns true if the element exists, else false. If the element is cached, it is looked up,
        /// else it is searched for (with SearchRoot as the search root Node)
        /// <para>If cache == true, then the element (if found) and all elements searched through will be cached</para>
        /// </summary>
        public bool HasElement(string element, bool cache = true)
        {
            return Element(element, cache) != null;
        }
        /// <summary>
        /// Finds an element by name. If the element is cached, it is looked up,
        /// else it is searched for (with SearchRoot as the search root Node)
        /// <para>If cache == true, then the element (if found) and all elements searched through will be cached</para>
        /// </summary>
        private XElement Element(string element, bool cache = true)
        {
            return Element(element, searchRoot, cache);
        }
        /// <summary>
        /// Finds an element by name. If the element is cached, it is looked up,
        /// else it is searched for (using the given root Node)
        /// <para>If cache == true, then the element (if found) and all elements searched through will be cached</para>
        /// </summary>
        private XElement Element(string element, XElement root, bool cache = true)
        {
            if (this.cache.ContainsKey(element))
                return this.cache[element];
            if (!cache)
                return root.DescendantsAndSelf().FirstOrDefault(e => e.Name.LocalName == element);
            var elts = root.DescendantsAndSelf();
            foreach(var elt in elts)
            {
                if (this.cache.ContainsKey(elt.Name.LocalName))
                    continue;
                this.cache.Add(elt.Name.LocalName, elt);
                if (elt.Name.LocalName == element)
                    return elt;
            }
            return null;
        }   
    }
}
