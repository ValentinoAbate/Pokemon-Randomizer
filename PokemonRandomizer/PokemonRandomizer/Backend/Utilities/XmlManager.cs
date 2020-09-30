using PokemonRandomizer.Backend.DataStructures;
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
        public const string numAttr = "num";
        public const string sizeAttr = "size";

        public XElement Root { get; }
        private XElement searchRoot;
        public XElement Constants
        {
            get
            {
                if (cache.ContainsKey("constants"))
                    return cache["constants"];
                return Element("constants", Root);
            }
        }

        private readonly Dictionary<string, XElement> cache = new Dictionary<string, XElement>();

        #region Constructors
        public XmlManager(string doc) : this(XDocument.Parse(doc).Root) { }
        public XmlManager(XElement root)
        {
            Root = root;
            searchRoot = root;
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
        /// <summary>
        /// Finds the offset of the specified data using various methods.
        /// If no method successfully finds a valid offset, return null.
        /// If "isAtValidOffset" == null, any offset in the rom is considered valid.
        /// TODO: Add pointer prefix method
        /// TODO: Check that the thing at the "pointer" location is actually a pointer
        /// </summary>
        public int? FindOffset(string element, Rom rom, Func<Rom, int, bool> isValidOffset = null)
        {
            if (HasPointer(element))
            {
                int offset = rom.ReadPointer(Pointer(element));
                if(rom.IsValidOffset(offset))
                {
                    if (isValidOffset == null || isValidOffset(rom, offset))
                        return offset;
                }
            }
            else if(HasOffset(element))
            {
                int offset = Offset(element);
                if (rom.IsValidOffset(offset))
                {
                    if (isValidOffset == null || isValidOffset(rom, offset))
                        return offset;
                }
            }
            return null;
        }
        /// <summary>
        /// Set the Rom's internal offset to the offset of the specified data (found using FindOffset).
        /// Returns false if no valid offset is found
        /// </summary>
        public bool FindAndSeekOffset(string element, Rom rom, Func<Rom, int, bool> isValidOffset = null)
        {
            int? offset = FindOffset(element, rom, isValidOffset);
            if(offset != null)
            {
                rom.Seek((int)offset);
                return true;
            }
            return false;
        }
        /// <summary> returns the "num" (amount of entries) attribute of the given element. Expects an integer value </summary>
        public int Num(string element)
        {
            return (int)Attr(element, numAttr);
        }
        /// <summary> returns the "size" (size in bytes) attribute of the given element. Expects an integer value </summary>
        public int Size(string element)
        {
            return (int)Attr(element, sizeAttr);
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
        /// <summary> returns the given attribute of the element converted from hex string to int </summary> 
        public int HexAttr(string element, string attribute)
        {
            return HexToInt((string)Attr(element, attribute));
        }
        /// <summary> returns the given attribute of the element converted from string to int </summary> 
        public int IntAttr(string element, string attribute)
        {
            return int.Parse(Attr(element, attribute).Value);
        }
        /// <summary> returns the given attribute of the element unpacked from an array </summary>
        public string[] ArrayAttr(string element, string attribute)
        {
            return ((string)Attr(element, attribute)).Trim('[', ']').Split(',');
        }
        /// <summary> returns the given attribute of the element with each element interpreted as an int[] </summary>
        public int[] IntArrayAttr(string element, string attribute)
        {
            return Array.ConvertAll(((string)Attr(element, attribute)).Trim('[', ']').Split(','), int.Parse);
        }
        /// <summary> returns the given attribute of the element with each element interpreted from a hex string </summary>
        public int[] HexArrayAttr(string element, string attribute)
        {
            return Array.ConvertAll(((string)Attr(element, attribute)).Trim('[', ']').Split(','), HexToInt);
        }
        /// <summary>
        /// Safely gets an array attribute if it exists. else returns an empty array.
        /// ArrayGetter should either be: ArrayAttr, IntArrayAttr, or HexArrayAttr
        /// </summary>
        public T[] SafeArrayAttr<T>(string element, string attribute, Func<string, string, T[]> arrayGetter)
        {
            if (!HasElementWithAttr(element, attribute))
                return new T[0];
            return arrayGetter(element, attribute);
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
        /// <summary>
        /// Finds an element by name and returns the given attribute.
        /// If the element is cached, it is looked up,
        /// else it is searched for (with SearchRoot as the search root Node)
        /// </summary>
        public XAttribute Attr(string element, string attribute)
        {
            return Element(element)?.Attribute(attribute);
        }
        /// <summary>
        /// Finds an element by name and returns the given attribute.
        /// If the element is cached, it is looked up,
        /// else it is searched for (with the specified search root as the search root Node)
        /// </summary>
        public XAttribute Attr(string element, string attribute, XElement root)
        {
            return Element(element, root)?.Attribute(attribute);
        }
        /// <summary>
        /// Returns tru if the element exists, else false. If the element is cached, it is looked up,
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
        public XElement Element(string element, bool cache = true)
        {
            return Element(element, searchRoot, cache);
        }
        /// <summary>
        /// Finds an element by name. If the element is cached, it is looked up,
        /// else it is searched for (using the given root Node)
        /// <para>If cache == true, then the element (if found) and all elements searched through will be cached</para>
        /// </summary>
        public XElement Element(string element, XElement root, bool cache = true)
        {
            if (this.cache.ContainsKey(element))
                return this.cache[element];
            if (!cache)
                return root.DescendantsAndSelf().FirstOrDefault(e => e.Name == element);
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
