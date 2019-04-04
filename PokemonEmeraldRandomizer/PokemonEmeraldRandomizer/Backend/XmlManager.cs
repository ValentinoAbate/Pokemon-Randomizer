using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PokemonEmeraldRandomizer.Backend
{
    public class XmlManager
    {       
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

        private Dictionary<string, XElement> cache = new Dictionary<string, XElement>();

        #region Constructors
        public XmlManager(string path) : this(XElement.Load(path)) { }
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
        /// <summary> returns the "num" (amount of entries) attribute of the given element </summary>
        public int Num(string element)
        {
            return (int)Attr(element, "num");
        }
        /// <summary> returns the "size" (size in bytes) attribute of the given element </summary>
        public int Size(string element)
        {
            return (int)Attr(element, "size");
        }
        /// <summary> returns the "offset" (offset) attribute of the element converted from hex string to int </summary>
        public int Offset(string element)
        {
            return HexAttr(element, "offset");
        }
        /// <summary> returns the given attribute of the element converted from hex string to int </summary> 
        public int HexAttr(string element, string attribute)
        {
            return HexToInt((string)Attr(element, attribute));
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
        /// <summary> returns the given element's content as a string </summary> 
        public string StringElt(string element)
        {
            return (string)Element(element);
        }
        #endregion

        /// <summary>
        /// Finds an element by name and returns the given attribute.
        /// If the element is cached, it is looked up,
        /// else it is searched for (with SearchRoot as the search root Node)
        /// </summary>
        public XAttribute Attr(string element, string attribute)
        {
            return Element(element).Attribute(attribute);
        }
        /// <summary>
        /// Finds an element by name and returns the given attribute.
        /// If the element is cached, it is looked up,
        /// else it is searched for (with the specified search root as the search root Node)
        /// </summary>
        public XAttribute Attr(string element, string attribute, XElement root)
        {
            return Element(element, root).Attribute(attribute);
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
