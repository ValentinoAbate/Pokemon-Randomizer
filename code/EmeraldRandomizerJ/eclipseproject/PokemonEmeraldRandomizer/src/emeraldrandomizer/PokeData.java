/*
 * Pokemon Emerald Randomizer, v.2.2
 * Date of release: 13 April 2014
 * Author: Artemis251
 * Thanks for takin' a peek!
 */

package emeraldrandomizer;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Random;

public class PokeData {
    private int[] stats;        //(hp/at/df/sp/sa/sd)
    private int[] types;
    private int[] evs;          //(hp/at/df/sp/sa/sd)
    private int[] abilities;
    private int[] heldItems;
    private int[] statSwaps;    //old stat places
    private int[] TMcompat;     //TM compatability
    private int[] tutorCompat;  //Move Tutor compatability
    private HashMap<Integer,Integer> attackHash;
    private ArrayList<Integer> availableMoves;
    private final int HELD_ITEM_CHANCE = 25;
    private final int DUAL_TYPE_CHANCE = 47;      // 171/386 are dual-typed (increased in case of double selection)
    private final int ADD_DUAL_TYPE_CHANCE = 21;  // 38/196 evolutions become dual-type (increased for double selection %)
    private final int DUAL_ABILITY_CHANCE = 50;

    public PokeData(){
        stats = new int[]{1,1,1,1,1,1};
        types = new int[]{0,0};
        evs = new int[]{0,0,0,0,0,0};
        abilities = new int[]{1,0};
        heldItems = new int[]{0,0};
        statSwaps = new int[]{0,1,2,3,4,5};
        TMcompat = new int[]{0,0,0,0,0,0,0,0};
        tutorCompat = new int[]{0,0,0,0};
    }

    public PokeData(int[] data){
        stats = new int[]{1,1,1,1,1,1};
        types = new int[]{0,0};
        evs = new int[]{0,0,0,0,0,0};
        abilities = new int[]{1,0};
        heldItems = new int[]{0,0};
        statSwaps = new int[]{0,1,2,3,4,5};
        TMcompat = new int[]{0,0,0,0,0,0,0,0};
        tutorCompat = new int[]{0,0,0,0};
        if (data.length < 28){
            System.out.println("Error parsing Pokemon Data: Expected 28 bytes, got " + data.length);
        } else{
            for (int i=0;i<6;i++){  //fill in stats (hp/at/df/sp/sa/sd)
                setStat(i,data[i]);
                evs[i] = (data[10 + i/4] >> ((i*2)%8))&3;
            }
            setType(0,data[6]);
            setType(1,data[7]);
            setAbility(0,data[22]);
            setAbility(1,data[23]);
            heldItems = new int[]{data[13]*256 + data[12],data[15]*256 + data[14]};
        }
    }

    private boolean isValidMatrix(int expectedSize, int actualSize, String matrix){
        if (actualSize < expectedSize){
            System.out.println("Invalid PokeData " + matrix + " allocation: Expected "
                    + expectedSize + "-byte matrix, found len " +
                    actualSize);
            return false;
        } return true;
    }

    public void setHP(int i){
        if (!isValidMatrix(6,stats.length,"stats")) return;
        setStat(0,i);
    }

    public void setAT(int i){
        if (!isValidMatrix(6,stats.length,"stats")) return;
        setStat(1,i);
    }

    public void setDF(int i){
        if (!isValidMatrix(6,stats.length,"stats")) return;
        setStat(2,i);
    }

    public void setSA(int i){
        if (!isValidMatrix(6,stats.length,"stats")) return;
        setStat(4,i);
    }

    public void setSD(int i){
        if (!isValidMatrix(6,stats.length,"stats")) return;
        setStat(5,i);
    }

    public void setSP(int i){
        if (!isValidMatrix(6,stats.length,"stats")) return;
        setStat(3,i);
    }

    private void setStat(int place, int value){
        if (value <= 0) {
            stats[place] = 1;
        } else if (value > 255){
            stats[place] = 255;
        } else{
            stats[place] = value;
        }
    }

    public void setStats(int hp, int at, int df, int sa, int sd, int sp){
        if (!isValidMatrix(6,stats.length,"stats")) return;
        setStat(0,hp);
        setStat(1,at);
        setStat(2,df);
        setStat(3,sp);
        setStat(4,sa);
        setStat(5,sd);
    }

    public void setType(int typeSlot, int i){
        if (!isValidMatrix(2,types.length,"types")) return;
        if (typeSlot > 1 || typeSlot <0){
            System.out.println("Invalid type slot: " + typeSlot + " -- type change ignored.");
            return;
        }
        if (i < 0 || i > 17 || i == 9) {
            System.out.println("Invalid type ignored: " + i + " -- set to Normal-type (0x00)");
            types[typeSlot] = 0;
        } else{
            types[typeSlot] = i;
        }
    }

    public void setTypes(int i, int j){
        if (!isValidMatrix(2,types.length,"types")) return;
        setType(0,i);
        setType(1,j);
    }

    public void setEVs(int hp, int at, int df, int sa, int sd, int sp){
        setEVs(new int[]{hp,at,df,sa,sd,sp});
    }

    public void setEVs(int[] evIn){
        if (!isValidMatrix(6,evs.length,"EVs")) return;
        for (int i=0;i<6;i++){
            if (evIn[i] < 0 || evIn[i] > 3){
                System.out.println("  > Erroneous EV value: " + evIn[i] + " -- 0 used instead.");
                evs[i] = 0;
            } else{
                evs[i] = evIn[i];
            }
        }
    }

    public void setAbilities(int abil1, int abil2){
        if (!isValidMatrix(2,abilities.length,"abilities")) return;
        setAbility(0,abil1);
        setAbility(1,abil2);
    }

    public void setAbility(int abilitySlot, int abil){
        if (!isValidMatrix(2,abilities.length,"abilities")) return;
        if (abilitySlot > 1 || abilitySlot < 0){
            System.out.println("Erroneous ability slot: " + abilitySlot + " -- ability change ignored.");
            return;
        }
        if (abil < 0 || abil > 77){
            System.out.println("  > Erroneous ability value: " + abil + " -- ability 01 (Stench) used instead.");
            abilities[abilitySlot] = 1;
        } else if (abilitySlot == 0 && abil == 0){
            System.out.println("  > Ability 1 cannot be 0 -- ability 01 (Stench) used instead.");
            abilities[abilitySlot] = 1;
        } else{
            abilities[abilitySlot] = abil;
        }
    }

    public void setHeldItem(int slot, int item){
        if (!isValidMatrix(2,heldItems.length,"held items")) return;
        if (slot > 1 || slot < 0){
            System.out.println("Erroneous held item slot: " + slot + " -- held item change ignored.");
            return;
        }
        if (item < 0){
            heldItems[slot] = 0;
        } else if (item > 346){
            heldItems[slot] = 346;
        } else{
            heldItems[slot] = item;
        }
    }

    public void setHeldItems(int item1, int item2){
        setHeldItem(0,item1);
        setHeldItem(1,item2);
    }

    public void setStatSwaps(int a, int b, int c, int d, int e, int f){
        setStatSwaps(new int[]{a,b,c,d,e,f});
    }

    public void setStatSwaps(int[] swaps){
        if (swaps.length != 6 || statSwaps.length != 6) {
            System.out.println("Error with stat swap array lengths. No stats swap data was retained.");
            return;
        }
        for (int i=0;i<statSwaps.length;i++){
            if (swaps[i] > 5 || swaps[i] < 0){
                System.out.println("Error with stat swap pointers. No stats swap data was retained.");
                return;
            }
        }
        for (int i=0;i<statSwaps.length;i++){
            statSwaps[i] = swaps[i];
        }
    }

    public void setTMCompatability(int[] inp){
        if (!isValidMatrix(8,TMcompat.length,"TM compatability")) return;
        for (int i=0;i<TMcompat.length;i++){
            TMcompat[i] = inp[i];
        }
    }

    public void setTutorCompatability(int[] inp){
        if (!isValidMatrix(4,tutorCompat.length,"Move Tutor compatability")) return;
        for (int i=0;i<tutorCompat.length;i++){
            tutorCompat[i] = inp[i];
        }
    }

    public void setAttackHash(HashMap<Integer,Integer> atkHash){
        attackHash = atkHash;
    }

    public void setAvailableMoves(ArrayList<Integer> movelist){
        availableMoves = movelist;
    }

    public int[] getStats(){
        return stats;
    }

    public int getStat(int slot){
        if (slot > 5 || slot < 0) return 1;
        return stats[slot];
    }

    public int[] getTypes(){
        return types;
    }

    public int getType(int slot){
        if (slot > 1 || slot < 0) return 0;
        return types[slot];
    }

    public int[] getEVs(){
        return evs;
    }

    public int getEV(int slot){
        if (slot > 5 || slot < 0) return 0;
        return evs[slot];
    }

    public int[] getAbilities(){
        return abilities;
    }

    public int getAbility(int slot){
        if (slot > 1 || slot < 0) return 0;
        return abilities[slot];
    }

    public int[] getItems(){
        return heldItems;
    }

    public int getItem(int slot){
        if (slot > 1 || slot < 0) return 0;
        return heldItems[slot];
    }

    public int[] getStatSwaps(){
        return statSwaps;
    }

    public int[] getTMCompatability(){
        return TMcompat;
    }

    public int[] getTutorCompatability(){
        return tutorCompat;
    }

    public ArrayList<Integer> getAvailableMoves(){
        return availableMoves;
    }

    public HashMap<Integer,Integer> getAttackHash(){
        return attackHash;
    }

    public void swapStats(){
        if (stats.length != 6 || statSwaps.length != 6 || evs.length != 6) {
            System.out.println("Error with stat swap array lengths. No stats were swapped.");
            return;
        }
        int[] statBak = new int[stats.length];
        int[] evsBak = new int[evs.length];
        for (int i=0;i<stats.length;i++){
            statBak[i] = stats[i];
            evsBak[i] = evs[i];
        }
        for (int i=0;i<stats.length;i++){
            stats[i] = statBak[statSwaps[i]];
            evs[i] = evsBak[statSwaps[i]];
        }
    }

    public void swapStats(int[] swaps){
        setStatSwaps(swaps);
        swapStats();
    }

    public void randomizeStats(){   //also randomizes EVs accordingly
        int[] statsBak = stats.clone();
        int[] evBak = evs.clone();
        ArrayList<Integer> slots = new ArrayList<Integer>();
        Random rand = new Random();
        for (int i=0;i<6;i++){
            slots.add(i);
        }
        int randChox;
        for (int i=0;i<6;i++){
            randChox = slots.remove(rand.nextInt(slots.size()));
            stats[i] = statsBak[randChox];
            evs[i] = evBak[randChox];
            statSwaps[i] = randChox;
        }
    }

    public void randomizeAbilities(){
        Random rand = new Random();
        ArrayList<Integer> alAbil = getAbilityList();
        abilities[0] = alAbil.remove(rand.nextInt(alAbil.size()));
        if (rand.nextInt(100) < DUAL_ABILITY_CHANCE){
            abilities[1] = alAbil.remove(rand.nextInt(alAbil.size()));
        } else{
            abilities[1] = abilities[0];
        }
    }

    public void randomizeTypes(){
        Random rand = new Random();
        ArrayList<Integer> alTypes = getTypeList();
        int baseType = alTypes.remove(rand.nextInt(alTypes.size()));
        types[0] = baseType;
        if (rand.nextInt(100) < DUAL_TYPE_CHANCE){
            types[1] = alTypes.remove(rand.nextInt(alTypes.size()));
        } else {
            types[1] = baseType;
        }

    }

    public void randomizeType(int typeSlot){
        Random rand = new Random();
        ArrayList<Integer> alTypes = getTypeList();
        if (rand.nextInt(100) <= ADD_DUAL_TYPE_CHANCE){
            types[typeSlot] = alTypes.remove(rand.nextInt(alTypes.size()));
        }
    }

    public void rerandomizeTypes(){
        if (types.length != 2){
            types = new int[]{0,0};
            System.out.println(" !! Types array re-cast to allow 2 slots.");
        }
        if (types[0] == types[1]){
            Random rand = new Random();
            ArrayList<Integer> alTypes = getTypeList();
            if (rand.nextInt(100) <= ADD_DUAL_TYPE_CHANCE){
                types[1] = alTypes.remove(rand.nextInt(alTypes.size()));
            }
        }
    }

    public void derandomizeTypes(){
        if (types.length != 2){
            types = new int[]{0,0};
            System.out.println(" !! Types array re-cast to allow 2 slots.");
        }
        if (types[0] != types[1]){
            Random rand = new Random();
            if (rand.nextInt(100) <= ADD_DUAL_TYPE_CHANCE){
                types[1] = types[0];
            }
        }
    }

    public void randomizeItems(){
        ArrayKeeper ak = new ArrayKeeper();
        ArrayList<Integer> alItems = ak.getArrayListInt(ak.getUsableItems());
        Random rand = new Random();
        for (int i=0;i<2;i++){
            if (rand.nextInt(100) < HELD_ITEM_CHANCE){
                heldItems[i] = alItems.remove(rand.nextInt(alItems.size()));
            } else{
                heldItems[i] = 0;
            }
        }
    }

    public void rerandomizeItems(){
        ArrayKeeper ak = new ArrayKeeper();
        ArrayList<Integer> alItems = ak.getArrayListInt(ak.getUsableItems());
        Random rand = new Random();
        for (int i=0;i<2;i++){
            if (heldItems[i] == 0 && rand.nextInt(100) < HELD_ITEM_CHANCE){
                heldItems[i] = alItems.remove(rand.nextInt(alItems.size()));
            }
        }
    }

    public void derandomizeItems(){
        Random rand = new Random();
        for (int i=0;i<2;i++){
            if (heldItems[i] != 0 && rand.nextInt(100) < HELD_ITEM_CHANCE){
                heldItems[i] = 0;
            }
        }
    }

    public void randomizeTMCompatability(){
        Random rand = new Random();
        for (int i=0;i<TMcompat.length;i++){
            TMcompat[i] = rand.nextInt(256);
        }
    }

    public void rerandomizeTMCompatability(){
        Random rand = new Random();
        for (int i=0;i<TMcompat.length;i++){// => 1/8 chance
            TMcompat[i] = TMcompat[i] | (rand.nextInt(256) & rand.nextInt(256) & rand.nextInt(256));
        }
    }

    public void derandomizeTMCompatability(){
        Random rand = new Random();
        for (int i=0;i<TMcompat.length;i++){// => 1/8 chance
            TMcompat[i] = TMcompat[i] & (~(rand.nextInt(256) & rand.nextInt(256) & rand.nextInt(256)) & 255);
        }
    }

    public void randomizeTutorCompatability(){
        Random rand = new Random();
        for (int i=0;i<tutorCompat.length;i++){
            tutorCompat[i] = rand.nextInt(256);
        }
        //tutorCompat[3] = tutorCompat[3] & 63; //last 2 bits are 0
    }

    public void rerandomizeTutorCompatability(){
        Random rand = new Random();
        for (int i=0;i<tutorCompat.length;i++){// => 1/4 chance
            tutorCompat[i] = tutorCompat[i] | (rand.nextInt(256) & rand.nextInt(256));
        }
    }

    public void derandomizeTutorCompatability(){
        Random rand = new Random();
        for (int i=0;i<tutorCompat.length;i++){// => 1/4 chance
            tutorCompat[i] = tutorCompat[i] & (~(rand.nextInt(256) & rand.nextInt(256)) & 255);
        }
    }

    public ArrayList<Integer> getAbilityList(){
        ArrayList<Integer> alOut = new ArrayList<Integer>();
        for (int i=1;i<=76;i++){
            alOut.add(i);
        }
        alOut.remove(new Integer(25)); //Wonder Guard
        alOut.remove(new Integer(59)); //Forecast
        return alOut;
    }

    public ArrayList<Integer> getTypeList(){
        ArrayList<Integer> alOut = new ArrayList<Integer>();
        for (int i=0;i<=17;i++){
            alOut.add(i);
        }
        alOut.remove(9);    //not a type
        return alOut;
    }

    @Override
     public String toString(){
        ArrayKeeper lk = new ArrayKeeper();
        StringBuilder sb = new StringBuilder();
        //try{
            for (int i=0;i<6;i++){
                if (stats[i]<10){
                    sb.append("  ");
                } else if (stats[i] < 100){
                    sb.append(" ");
                }
                sb.append(stats[i] + " ");
            }
            sb.append("  |  ");
            for (int i=0;i<6;i++){
                if (evs[i]==0){
                    sb.append("- ");
                } else{
                    sb.append(evs[i] + " ");
                }
            }
            sb.append("  | ");
            String[] typeList = lk.getTypeList();
            sb.append(typeList[types[0]]);
            if (types[0] == types[1]){
                sb.append("     |  ");
            } else{
                sb.append("/" + typeList[types[1]] + " |  ");
            }
            String[] abilityList = lk.getAbilityList();
            for (int j=0;j<2;j++){
                String abil = abilityList[abilities[j]];
                sb.append(abil);
                for (int i=abil.length();i<=12;i++){
                    sb.append(" ");
                } sb.append(" | ");
            }
            String[] itemList = lk.getItemList();
            for (int j=0;j<2;j++){
                String item = itemList[heldItems[j]];
                sb.append(item);
                for (int i=item.length();i<=12;i++){
                    sb.append(" ");
                } sb.append(" | ");
            }

            return sb.toString();
        /*} catch (ArrayIndexOutOfBoundsException aioobe){
            return " >< ERROR! PokeData arrays not normal size!";
        }*/
     }

    public PokeData getClone(){
        PokeData out = new PokeData();
        out.setStats(stats[0],stats[1],stats[2],stats[3],stats[4],stats[5]);
        out.setEVs(evs);
        out.setAbilities(abilities[0], abilities[1]);
        out.setStatSwaps(statSwaps);
        out.setTypes(types[0], types[1]);
        out.setHeldItems(heldItems[0], heldItems[1]);
        out.setTMCompatability(TMcompat);
        out.setAvailableMoves(availableMoves);
        out.setAttackHash(attackHash);
        return out;
    }
}
