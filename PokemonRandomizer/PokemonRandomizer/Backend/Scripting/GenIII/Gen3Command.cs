using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.Scripting.GenIII
{
    public class Gen3Command : DataStructures.Scripts.Command
    {
        public const int itemTypeVar = 0x8000;
        public const int itemQuantityVar = 0x8001;
        public const int eventPokemonSpeciesVar = 0x8004;
        public const int eventPokemonLevelVar = 0x8005;
        public const int eventPokemonItemVar = 0x8006;
        public const int moveTutorIndexVar = 0x8005;

        #region Command Constants

        public const byte nop                                 = 0x00;
        public const byte nop1                                = 0x01;
        public const byte end                                 = 0x02;
        public const byte @return                             = 0x03;
        public const byte call                                = 0x04;
        public const byte @goto                               = 0x05;
        public const byte gotoif                              = 0x06;
        public const byte callif                              = 0x07;
        public const byte gotostd                             = 0x08;
        public const byte callstd                             = 0x09;
        public const byte gotostdif                           = 0x0A;
        public const byte callstdif                           = 0x0B;
        public const byte jumpram                             = 0x0C;
        public const byte killscript                          = 0x0D;
        public const byte setByte                             = 0x0E;
        public const byte loadpointer                         = 0x0F;
        public const byte setbyte2                            = 0x10;
        public const byte writebytetooffset                   = 0x11;
        public const byte loadbytefrompointer                 = 0x12;
        public const byte setfarbyte                          = 0x13;
        public const byte copyscriptbanks                     = 0x14;
        public const byte copybyte                            = 0x15;
        public const byte setvar                              = 0x16;
        public const byte addvar                              = 0x17;
        public const byte subvar                              = 0x18;
        public const byte copyvar                             = 0x19;
        public const byte copyvarifnotzero                    = 0x1A;                           
        public const byte comparebanks                        = 0x1B;
        public const byte comparebanktobyte                   = 0x1C;
        public const byte compareBankTofarbyte                = 0x1D;
        public const byte compareFarByteToBank                = 0x1E;
        public const byte compareFarByteToByte                = 0x1F;
        public const byte compareFarBytes                     = 0x20;
        public const byte compare                             = 0x21;
        public const byte comparevars                         = 0x22;
        public const byte callasm                             = 0x23;
        public const byte setcode                             = 0x24;
        public const byte special                             = 0x25;
        public const byte special2                            = 0x26;
        public const byte waitstate                           = 0x27;
        public const byte pause                               = 0x28;
        public const byte setflag                             = 0x29;
        public const byte clearflag                           = 0x2A;
        public const byte checkflag                           = 0x2B;
        public const byte nop2Cfrlg                           = 0x2C;
        public const byte initclockrse                        = 0x2C;
        public const byte checkdailyflags                     = 0x2D;
        public const byte resetvars                           = 0x2E;
        public const byte sound                               = 0x2F;
        public const byte waitsound                           = 0x30;
        public const byte fanfare                             = 0x31;
        public const byte waitfanfare                         = 0x32;
        public const byte playsong                            = 0x33;
        public const byte playsong2                           = 0x34;
        public const byte fadedefault                         = 0x35;
        public const byte fadesong                            = 0x36;
        public const byte fadeout                             = 0x37;
        public const byte fadein                              = 0x38;
        public const byte warp                                = 0x39;
        public const byte warpmuted                           = 0x3A;
        public const byte warpwalk                            = 0x3B;
        public const byte warphole                            = 0x3C;
        public const byte warpteleport                        = 0x3D;
        public const byte warp3                               = 0x3E;
        public const byte setwarpplace                        = 0x3F;
        public const byte warp4                               = 0x40;
        public const byte warp5                               = 0x41;
        public const byte getplayerpos                        = 0x42;
        public const byte countPokemon                        = 0x43;
        public const byte additem                             = 0x44;                               
        public const byte removeitem                          = 0x45;
        public const byte checkitemroom                       = 0x46;
        public const byte checkitem                           = 0x47;
        public const byte checkitemtype                       = 0x48;
        public const byte addpcitem                           = 0x49;
        public const byte checkpcitem                         = 0x4A;
        public const byte adddecoration                       = 0x4B;                                       
        public const byte removedecoration                    = 0x4C;
        public const byte testdecoration                      = 0x4D;
        public const byte checkdecoration                     = 0x4E;  
        public const byte applymovement                       = 0x4F;                                    
        public const byte applymovement2                      = 0x50;
        public const byte waitmovement                        = 0x51;
        public const byte waitmovementpos                     = 0x52;
        public const byte hidesprite                          = 0x53;
        public const byte hidespritepos                       = 0x54;
        public const byte showsprite                          = 0x55;
        public const byte showspritepos                       = 0x56;
        public const byte movesprite                          = 0x57;
        public const byte spritevisible                       = 0x58;
        public const byte spriteinvisible                     = 0x59;
        public const byte faceplayer                          = 0x5A;
        public const byte spriteface                          = 0x5B;                                 
        public const byte trainerbattle                       = 0x5C;                                  
        public const byte repeattrainerbattle                 = 0x5D;
        public const byte endtrainerbattle                    = 0x5E;
        public const byte endtrainerbattle2                   = 0x5F;
        public const byte checktrainerflag                    = 0x60;
        public const byte cleartrainerflag                    = 0x61;
        public const byte settrainerflag                      = 0x62;
        public const byte movesprite2                         = 0x63;
        public const byte moveoffscreen                       = 0x64;
        public const byte spritebehave                        = 0x65;
        public const byte waitmsg                             = 0x66;
        public const byte preparemsg                          = 0x67;                                     
        public const byte closeonkeypress                     = 0x68;
        public const byte lockall                             = 0x69;
        public const byte @lock                               = 0x6A;   
        public const byte releaseall                          = 0x6B;
        public const byte release                             = 0x6C;
        public const byte waitkeypress                        = 0x6D;
        public const byte yesnobox                            = 0x6E;
        public const byte multichoice                         = 0x6F;
        public const byte multichoice2                        = 0x70;
        public const byte multichoice3                        = 0x71;
        public const byte showbox                             = 0x72;
        public const byte hidebox                             = 0x73;
        public const byte clearbox                            = 0x74;
        public const byte showpokepic                         = 0x75;
        public const byte hidepokepic                         = 0x76;
        public const byte showcontestwinner                   = 0x77;
        public const byte braille                             = 0x78;  
        public const byte givePokemon                         = 0x79;                                    
        public const byte giveEgg                             = 0x7A;
        public const byte setpkmnpp                           = 0x7B;  
        public const byte checkattack                         = 0x7C;                                     
        public const byte bufferPokemon                       = 0x7D;
        public const byte bufferfirstPokemon                  = 0x7E;
        public const byte bufferpartyPokemon                  = 0x7F;
        public const byte bufferitem                          = 0x80;
        public const byte bufferdecoration                    = 0x81;
        public const byte bufferattack                        = 0x82;
        public const byte buffernumber                        = 0x83;
        public const byte bufferstd                           = 0x84;
        public const byte bufferstring                        = 0x85;
        public const byte pokemart                            = 0x86;
        public const byte pokemart2                           = 0x87;
        public const byte pokemart3                           = 0x88;
        public const byte pokecasino                          = 0x89;
        public const byte nop8Afrlg                           = 0x8A;
        public const byte setberrytreerse                     = 0x8A;
        public const byte choosecontextpkmn                   = 0x8B;
        public const byte startcontest                        = 0x8C;
        public const byte showcontestresults                  = 0x8D;
        public const byte contestlinktransfer                 = 0x8E;
        public const byte random                              = 0x8F;
        public const byte givemoney                           = 0x90;
        public const byte paymoney                            = 0x91;
        public const byte checkmoney                          = 0x92;
        public const byte showmoney                           = 0x93;
        public const byte hidemoney                           = 0x94;
        public const byte updatemoney                         = 0x95;
        public const byte getpricereductionrse                = 0x96;
        public const byte nop96frlg                           = 0x96;
        public const byte fadescreen                          = 0x97;
        public const byte fadescreendelay                     = 0x98;
        public const byte darken                              = 0x99;
        public const byte lighten                             = 0x9A;
        public const byte preparemsg2                         = 0x9B;
        public const byte doanimation                         = 0x9C;
        public const byte setanimation                        = 0x9D;
        public const byte checkanimation                      = 0x9E;
        public const byte sethealingplace                     = 0x9F;
        public const byte checkgender                         = 0xA0;
        public const byte cry                                 = 0xA1;
        public const byte setmaptile                          = 0xA2;                   
        public const byte resetweather                        = 0xA3;
        public const byte setweather                          = 0xA4;
        public const byte doweather                           = 0xA5;
        public const byte changewalktile                      = 0xA6;
        public const byte setmapfooter                        = 0xA7;
        public const byte spritelevelup                       = 0xA8;
        public const byte restorespritelevel                  = 0xA9;
        public const byte createsprite                        = 0xAA;
        public const byte spriteface2                         = 0xAB;
        public const byte setdooropened                       = 0xAC;
        public const byte setdoorclosed                       = 0xAD;
        public const byte doorchange                          = 0xAE;
        public const byte setdooropened2                      = 0xAF;
        public const byte setdoorclosed2                      = 0xB0;
        public const byte nopB1                               = 0xB1; // addelevmenuitem RS
        public const byte nopB2                               = 0xB2; // showelevmenu RS
        public const byte checkcoins                          = 0xB3;
        public const byte givecoins                           = 0xB4;
        public const byte removecoins                         = 0xB5;
        public const byte setwildbattle                       = 0xB6;
        public const byte dowildbattle                        = 0xB7;
        public const byte setvirtualaddress                   = 0xB8;
        public const byte virtualgoto                         = 0xB9;
        public const byte virtualcall                         = 0xBA;
        public const byte virtualgotoif                       = 0xBB;
        public const byte virtualcallif                       = 0xBC;
        public const byte virtualmsgbox                       = 0xBD;
        public const byte virtualloadpointer                  = 0xBE;
        public const byte virtualbuffer                       = 0xBF;
        public const byte showcoins                           = 0xC0;
        public const byte hidecoins                           = 0xC1;
        public const byte updatecoins                         = 0xC2;
        public const byte incrementhiddenvalue                = 0xC3;
        public const byte warp6                               = 0xC4;
        public const byte waitcry                             = 0xC5;
        public const byte bufferboxname                       = 0xC6;
        public const byte textcolor                           = 0xC7;
        public const byte helptext                            = 0xC8;
        public const byte helptext2                           = 0xC9;
        public const byte signmsg                             = 0xCA;
        public const byte normalmsg                           = 0xCB;
        public const byte comparehiddenvar                    = 0xCC;
        public const byte setobedience                        = 0xCD;
        public const byte checkobedience                      = 0xCE;
        public const byte executeram                          = 0xCF;
        public const byte setworldmapflag                     = 0xD0;
        public const byte warpteleport2                       = 0xD1;
        public const byte setcatchlocation                    = 0xD2;
        public const byte braillelengthfrlg                   = 0xD3; 
        public const byte moverotatingtilesem                 = 0xD3; 
        public const byte bufferitemsfrlg                     = 0xD4;
        public const byte turnrotatingtilesem                 = 0xD4;
        public const byte initrotatingtilesem                 = 0xD5;
        public const byte freerotatingtilesem                 = 0xD6;
        public const byte warpmossdeepgymem                   = 0xD7;
        public const byte selectapproachingtr                 = 0xD8;
        public const byte lockfortrainerem                    = 0xD9;
        public const byte closebraillemessage                 = 0xDA;
        public const byte preparemsg3                         = 0xDB;
        public const byte fadescreen3                         = 0xDC;
        public const byte buffertrainerclass                  = 0xDD;
        public const byte buffertrainername                   = 0xDE;
        public const byte pokenavcall                         = 0xDF;
        public const byte warp8                               = 0xE0;
        public const byte buffercontesttype                   = 0xE1;
        public const byte bufferitems2                        = 0xE2;

        #endregion

        #region Argument Formats

        public enum Arg : ushort
        {
            Byte,
            Word,
            Long,
            Pointer,
            TrainerBattle,
        }

        private static readonly Arg[] noArgs = new Arg[0];
        private static readonly Arg[] byte1 = new Arg[] { Arg.Byte };
        private static readonly Arg[] byteWord = new Arg[] { Arg.Byte, Arg.Word };
        private static readonly Arg[] byteWord2 = new Arg[] { Arg.Byte, Arg.Word, Arg.Word };
        private static readonly Arg[] byteLong = new Arg[] { Arg.Byte, Arg.Long };
        private static readonly Arg[] byte2 = Enumerable.Repeat(Arg.Byte, 2).ToArray();
        private static readonly Arg[] byte2Word = new Arg[] { Arg.Byte, Arg.Byte, Arg.Word };
        private static readonly Arg[] byte3 = Enumerable.Repeat(Arg.Byte, 3).ToArray();
        private static readonly Arg[] byte4 = Enumerable.Repeat(Arg.Byte, 4).ToArray();
        private static readonly Arg[] bytePointer = new Arg[] { Arg.Byte, Arg.Pointer };
        private static readonly Arg[] pointerByte = new Arg[] { Arg.Pointer, Arg.Byte };
        private static readonly Arg[] pointer2 = new Arg[] { Arg.Pointer, Arg.Pointer };
        private static readonly Arg[] pointer = new Arg[] { Arg.Pointer };
        private static readonly Arg[] word = new Arg[] { Arg.Word };
        private static readonly Arg[] word2 = new Arg[] { Arg.Word, Arg.Word };
        private static readonly Arg[] word3 = new Arg[] { Arg.Word, Arg.Word, Arg.Word };
        private static readonly Arg[] word4 = Enumerable.Repeat(Arg.Word, 4).ToArray();
        private static readonly Arg[] wordByte = new Arg[] { Arg.Word, Arg.Byte };
        private static readonly Arg[] wordByte2 = new Arg[] { Arg.Word, Arg.Byte, Arg.Byte };
        private static readonly Arg[] wordByte3 = new Arg[] { Arg.Word, Arg.Byte, Arg.Byte, Arg.Byte };
        private static readonly Arg[] wordByteWord = new Arg[] { Arg.Word, Arg.Byte, Arg.Word };
        private static readonly Arg[] wordPointer = new Arg[] { Arg.Word, Arg.Pointer };
        private static readonly Arg[] word2Pointer = new Arg[] { Arg.Word, Arg.Word, Arg.Pointer };
        private static readonly Arg[] word2Pointer2 = new Arg[] { Arg.Word, Arg.Word, Arg.Pointer, Arg.Pointer };
        private static readonly Arg[] word2Pointer3 = new Arg[] { Arg.Word, Arg.Word, Arg.Pointer, Arg.Pointer, Arg.Pointer };
        private static readonly Arg[] word2Pointer4 = new Arg[] { Arg.Word, Arg.Word, Arg.Pointer, Arg.Pointer, Arg.Pointer, Arg.Pointer };
        private static readonly Arg[] long1 = new Arg[] { Arg.Long };
        private static readonly Arg[] longByte = new Arg[] { Arg.Long, Arg.Byte };
        private static readonly Arg[] warpArgs = new Arg[] { Arg.Byte, Arg.Byte, Arg.Byte, Arg.Word, Arg.Word };
        private static readonly Arg[] trainer = new Arg[] { Arg.TrainerBattle };
        private static readonly Arg[] givePokemonArgs = new Arg[] { Arg.Word, Arg.Byte, Arg.Word, Arg.Byte, Arg.Long, Arg.Long };
        private static readonly Arg[] createSpriteArgs = new Arg[] { Arg.Byte, Arg.Byte, Arg.Word, Arg.Word, Arg.Byte, Arg.Byte };

        #endregion

        #region Special Code Constants

        public const int specialGiveNationalDexFrlg = 0x16F;
        public const int specialSetWildEventPokemonFrlg = 0x1BB;
        public const int specialSetWildEventPokemonEmerald = 0x1E2;
        public const int specialGiveRegionalDexFrlg = 0x181;
        public const int specialGiveNationalDexEmerald = 0x1F3;
        public const int specialSelectMonForMoveTutorEmerald = 0x1DD;
        public const int specialSelectMonForMoveTutorFrlg = 0x18D;

        #endregion

        public static readonly Dictionary<byte, Arg[]> commandMap = new()
        {
            {nop                 , noArgs            },
            {nop1                , noArgs            },
            {end                 , noArgs            },
            {@return             , noArgs            },
            {call                , pointer           },
            {@goto               , pointer           },
            {callif              , bytePointer       },
            {gotoif              , bytePointer       },
            {callstd             , byte1             },
            {gotostd             , byte1             },
            {callstdif           , byte2             },
            {gotostdif           , byte2             },
            {jumpram             , noArgs            },
            {killscript          , noArgs            },
            {setByte             , byte1             },
            {loadpointer         , bytePointer       },
            {setbyte2            , byte2             },
            {writebytetooffset   , bytePointer       },
            {loadbytefrompointer , bytePointer       },
            {setfarbyte          , bytePointer       },
            {copyscriptbanks     , byte2             },
            {copybyte            , pointer2          },
            {setvar              , word2             },
            {addvar              , word2             },
            {subvar              , word2             },
            {copyvar             , word2             },
            {copyvarifnotzero    , word2             },
            {comparebanks        , byte2             },
            {comparebanktobyte   , byte2             },
            {compareBankTofarbyte, bytePointer       },
            {compareFarByteToBank, pointerByte       },
            {compareFarByteToByte, pointerByte       },
            {compareFarBytes     , pointer2          },
            {compare             , word2             },
            {comparevars         , word2             },
            {callasm             , pointer           },
            {setcode             , pointer           },
            {special             , word              }, // Special Fire Red Doc: https://www.pokecommunity.com/showthread.php?t=184273
            {special2            , word2             },
            {waitstate           , noArgs            },
            {pause               , word              },
            {setflag             , word              },
            {clearflag           , word              },
            {checkflag           , word              },
            {checkdailyflags     , noArgs            },
            {resetvars           , noArgs            },
            {sound               , word              },
            {waitsound           , noArgs            },
            {fanfare             , word              },
            {waitfanfare         , noArgs            },
            {playsong            , wordByte          },
            {playsong2           , word              },
            {fadedefault         , noArgs            },
            {fadesong            , word              },
            {fadeout             , byte1             },
            {fadein              , byte1             },
            {warp                , warpArgs          },
            {warpmuted           , warpArgs          },
            {warpwalk            , warpArgs          },
            {warphole            , byte2             },
            {warpteleport        , warpArgs          },
            {warp3               , warpArgs          },
            {setwarpplace        , warpArgs          },
            {warp4               , warpArgs          },
            {warp5               , warpArgs          },
            {getplayerpos        , word2             },
            {countPokemon        , noArgs            },
            {additem             , word2             },
            {removeitem          , word2             },
            {checkitemroom       , word2             },
            {checkitem           , word2             },
            {checkitemtype       , word              },
            {addpcitem           , word2             },
            {checkpcitem         , word2             },
            {adddecoration       , word              },
            {removedecoration    , word              },
            {testdecoration      , word              },
            {checkdecoration     , word              },
            {applymovement       , wordPointer       },
            {applymovement2      , wordPointer       },
            {waitmovement        , word              },
            {waitmovementpos     , wordByte2         },
            {hidesprite          , word              },
            {hidespritepos       , wordByte2         },
            {showsprite          , word              },
            {showspritepos       , wordByte2         },
            {movesprite          , word3             },
            {spritevisible       , wordByte2         },
            {spriteinvisible     , wordByte2         },
            {faceplayer          , noArgs            },
            {spriteface          , wordByte          },
            {trainerbattle       , trainer           },
            {repeattrainerbattle , noArgs            },
            {endtrainerbattle    , noArgs            },
            {endtrainerbattle2   , noArgs            },
            {checktrainerflag    , word              },
            {cleartrainerflag    , word              },
            {settrainerflag      , word              },
            {movesprite2         , word3             },
            {moveoffscreen       , word              },
            {spritebehave        , wordByte          },
            {waitmsg             , noArgs            },
            {preparemsg          , pointer           },
            {closeonkeypress     , noArgs            },
            {lockall             , noArgs            },
            {@lock               , noArgs            },
            {releaseall          , noArgs            },
            {release             , noArgs            },
            {waitkeypress        , noArgs            },
            {yesnobox            , byte2             },
            {multichoice         , byte4             },
            {multichoice2        , byte4             },
            {multichoice3        , byte4             },
            {showbox             , byte4             },
            {hidebox             , byte4             },
            {clearbox            , byte4             },
            {showpokepic         , wordByte2         },
            {hidepokepic         , noArgs            },
            {showcontestwinner   , byte1             },
            {braille             , pointer           },
            {givePokemon         , givePokemonArgs   },
            {giveEgg             , word              },
            {setpkmnpp           , byte2Word         },
            {checkattack         , word              },
            {bufferPokemon       , byteWord          },
            {bufferfirstPokemon  , byte1             },
            {bufferpartyPokemon  , byte2             },
            {bufferitem          , byteWord          },
            {bufferdecoration    , byteWord          },
            {bufferattack        , byteWord          },
            {buffernumber        , byteWord          },
            {bufferstd           , byteWord          },
            {bufferstring        , bytePointer       },
            {pokemart            , pointer           },
            {pokemart2           , pointer           },
            {pokemart3           , pointer           },
            {pokecasino          , word              },
            {choosecontextpkmn   , noArgs            },
            {startcontest        , noArgs            },
            {showcontestresults  , noArgs            },
            {contestlinktransfer , noArgs            },
            {random              , word              },
            {givemoney           , longByte          },
            {paymoney            , longByte          },
            {checkmoney          , longByte          },
            {showmoney           , byte2             },
            {hidemoney           , byte2             },
            {updatemoney         , byte2             },
            {fadescreen          , byte1             },
            {fadescreendelay     , byte2             },
            {darken              , word              },
            {lighten             , byte1             },
            {preparemsg2         , pointer           },
            {doanimation         , word              },
            {setanimation        , byteWord          },
            {checkanimation      , word              },
            {sethealingplace     , word              },
            {checkgender         , noArgs            },
            {cry                 , word2             },
            {setmaptile          , word4             },
            {resetweather        , noArgs            },
            {setweather          , word              },
            {doweather           , noArgs            },
            {changewalktile      , byte1             },
            {setmapfooter        , word              },
            {spritelevelup       , wordByte3         },
            {restorespritelevel  , wordByte2         },
            {createsprite        , createSpriteArgs  },
            {spriteface2         , byte2             },
            {setdooropened       , word2             },
            {setdoorclosed       , word2             },
            {doorchange          , noArgs            },
            {setdooropened2      , word2             },
            {setdoorclosed2      , word2             },
            {nopB1               , noArgs            }, // seems to have args in Em: addelevmenuitem a:req, b:req, c:req, d:req (byte word3)
            {nopB2               , noArgs            },
            {checkcoins          , word              },
            {givecoins           , word              },
            {removecoins         , word              },
            {setwildbattle       , wordByteWord      },
            {dowildbattle        , noArgs            },
            {setvirtualaddress   , long1             },
            {virtualgoto         , pointer           },
            {virtualcall         , pointer           },
            {virtualgotoif       , bytePointer       },
            {virtualcallif       , bytePointer       },
            {virtualmsgbox       , pointer           },
            {virtualloadpointer  , pointer           },
            {virtualbuffer       , bytePointer       },
            {showcoins           , byte2             },
            {hidecoins           , byte2             },
            {updatecoins         , byte2             },
            {incrementhiddenvalue, byte1             },
            {warp6               , warpArgs          },
            {waitcry             , noArgs            },
            {bufferboxname       , wordByte          },
            {textcolor           , byte1             },
            {helptext            , pointer           },
            {helptext2           , noArgs            },
            {signmsg             , noArgs            },
            {normalmsg           , noArgs            },
            {comparehiddenvar    , byteLong          },
            {setobedience        , word              },
            {checkobedience      , word              },
            {executeram          , noArgs            },
            {setworldmapflag     , word              },
            {warpteleport2       , warpArgs          },
            {setcatchlocation    , wordByte          },
            {initrotatingtilesem , word              },
            {freerotatingtilesem , noArgs            },
            {warpmossdeepgymem   , warpArgs          },
            {selectapproachingtr , noArgs            }, // selectapproachingtrainer
            {lockfortrainerem    , noArgs            },
            {closebraillemessage , noArgs            },
            {preparemsg3         , pointer           },
            {fadescreen3         , byte1             },
            {buffertrainerclass  , wordByte          },
            {buffertrainername   , wordByte          },
            {pokenavcall         , pointer           },
            {warp8               , warpArgs          },
            {buffercontesttype   , wordByte          },
            {bufferitems2        , byteWord2         },
        };

        public static readonly Dictionary<byte, Arg[]> frlgCommandMap = new()
        {
            {nop2Cfrlg           , noArgs            },
            {nop8Afrlg           , noArgs            },
            {nop96frlg           , noArgs            },
            {braillelengthfrlg   , pointer           },
            {bufferitemsfrlg     , byteWord2         }, // Byte word 2
        };

        public static readonly Dictionary<byte, Arg[]> rseCommandMap = new()
        {
            {initclockrse        , word2             }, // Initialize the RTC
            {setberrytreerse     , byte3             },
            {getpricereductionrse, word              }, // Gets the price reduction for the index given. 
            {moverotatingtilesem , word              }, // In Emerald For the rotating tile puzzles in Mossdeep Gym / Trick House Room 7. Moves the objects one rotation on the colored puzzle specified by puzzleNumber.
            {turnrotatingtilesem , noArgs            }, // For the rotating tile puzzles in Mossdeep Gym / Trick House Room 7. Updates the facing direction of all objects on the puzzle tiles
        };

        public static Arg[] GetTrainerArgs(int trainerType)
        {
            if (trainerCommandMap.TryGetValue(trainerType, out var trainerArgList))
                return trainerArgList;
            return trainerCommandMap[0x00]; // If the key isn't in the dictionary, use the default format
        }

        private static readonly Dictionary<int, Arg[]> trainerCommandMap = new Dictionary<int, Arg[]>
        {
            {0x00, word2Pointer2},
            {0x01, word2Pointer3},
            {0x02, word2Pointer3},
            {0x03, word2Pointer },
            {0x04, word2Pointer3},
            {0x05, word2Pointer2},
            {0x06, word2Pointer4},
            {0x07, word2Pointer3},
            {0x08, word2Pointer4},
            {0x09, word2Pointer2},
        };

        public int Size
        {
            get
            {
                int size = 1;
                foreach (var arg in args)
                {
                    size += arg.type switch
                    {
                        Arg.Byte => 1,
                        Arg.Word => 2,
                        Arg.Long or Arg.Pointer => 4,
                        _ => throw new System.NotImplementedException(),
                    };
                }
                return size;
            }
        }

        public byte code;
        public List<Argument> args = new List<Argument>(8);

        public int ArgData(int index) => args[index].data;
        public struct Argument 
        { 
            public int data;
            public readonly Arg type;

            public Argument(int data, Arg type)
            {
                this.data = data;
                this.type = type;
            }

            public override string ToString()
            {
                return data.ToString("X2") + " (" + type.ToString() + ")";
            }
        }

        public override string ToString()
        {
            if (args.Count <= 0)
                return code.ToString("X2");
            return code.ToString("X2") + ": " + args.Select(a => a.ToString()).Aggregate((s1, s2) => s1 + ", " + s2);
        }
    }
}
