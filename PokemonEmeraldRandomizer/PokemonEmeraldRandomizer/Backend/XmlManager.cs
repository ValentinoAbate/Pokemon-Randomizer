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
        private readonly XElement root;
        private XElement searchRoot;

        private Dictionary<string, XElement> cache = new Dictionary<string, XElement>();

        #region Constructors
        public XmlManager(string path) : this(XElement.Load(path)) { }
        public XmlManager(XElement root)
        {
            this.root = root;
            searchRoot = root;
        }
        #endregion

        public void SetSearchRoot(string element)
        {
            searchRoot = root;
            searchRoot = Element(element, false);
        }
        public void SetSearchRoot(XElement element)
        {
            searchRoot = element;
        }
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
        /// <summary> returns the "addy" (address) attribute of the element converted from hex string to int </summary>
        public int Addy(string element)
        {
            return HexAttr(element, "addy");
        }
        /// <summary> returns the given attribute of the element converted from hex string to int </summary> 
        public int HexAttr(string element, string attribute)
        {
            return HexToInt((string)Attr(element, attribute));
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
        /// else it is searched for (starting at the SearchRoot Node)
        /// </summary>
        public XAttribute Attr(string element, string attribute)
        {
            return Element(element).Attribute(attribute);
        }
        public XElement Element(string element, bool cache = true)
        {
            return Element(element, searchRoot, cache);
        }
        public XElement Element(string element, XElement start, bool cache = true)
        {
            if (this.cache.ContainsKey(element))
                return this.cache[element];
            if (!cache)
                return start.DescendantsAndSelf().FirstOrDefault(e => e.Name == element);
            var elts = start.DescendantsAndSelf();
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
