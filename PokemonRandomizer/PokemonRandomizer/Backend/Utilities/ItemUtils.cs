using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.Utilities
{
    using EnumTypes;
    using PokemonRandomizer.Backend.DataStructures;

    public static class ItemUtils
    {
        private static readonly HashSet<Item> mail = new HashSet<Item>
        {
            Item.Bead_Mail,
            Item.Dream_Mail,
            Item.Fab_Mail,
            Item.Glitter_Mail,
            Item.Harbor_Mail,
            Item.Mech_Mail,
            Item.Orange_Mail,
            Item.Retro_Mail,
            Item.Shadow_Mail,
            Item.Tropic_Mail,
            Item.Wave_Mail,
            Item.Wood_Mail,
        };
        public static bool IsMail(this ItemData item)
        {
            return IsMail(item.Item);
        }
        public static bool IsMail(this Item item)
        {
            return mail.Contains(item);
        }
        private static readonly HashSet<Item> tms = new HashSet<Item>
        {
            Item.TM01, Item.TM02, Item.TM03, Item.TM04, Item.TM05,
            Item.TM06, Item.TM07, Item.TM08, Item.TM09, Item.TM10,
            Item.TM11, Item.TM12, Item.TM13, Item.TM14, Item.TM15,
            Item.TM16, Item.TM17, Item.TM18, Item.TM19, Item.TM20,
            Item.TM21, Item.TM22, Item.TM23, Item.TM24, Item.TM25,
            Item.TM26, Item.TM27, Item.TM28, Item.TM29, Item.TM30,
            Item.TM31, Item.TM32, Item.TM33, Item.TM34, Item.TM35,
            Item.TM36, Item.TM37, Item.TM38, Item.TM39, Item.TM40,
            Item.TM41, Item.TM42, Item.TM43, Item.TM44, Item.TM45,
            Item.TM46, Item.TM47, Item.TM48, Item.TM49, Item.TM50,
        };
        public static bool IsTM(this ItemData item)
        {
            return IsTM(item.Item);
        }
        public static bool IsTM(this Item item)
        {
            return tms.Contains(item);
        }
    }
}
