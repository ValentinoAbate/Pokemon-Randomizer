/*
 * Pokemon Emerald Randomizer, v.2.2
 * Date of release: 13 April 2014
 * Author: Artemis251
 * Thanks for takin' a peek!
 */

package emeraldrandomizer;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.Random;

public class PokePalette {
    private final int VARIA = 10;                   //3 of these fuel the variation for sibling palettes
    private final int VARIABASE = 3;                // base forced change for sibling palettes
    private final int LIGHTDARK_CHANCE = 20;//20;        //% chance to be light or dark
    private final int TOTAL_CHANGE_THRESHOLD = 9;  //threshold for color changes to meet before stopping their seed
    private final int LIGHT = 1;
    private final int DARK = 2;
    private final int RAND_LIGHT = 3;
    private final int RAND_DARK = 4;
    private final int EMPTY = 0;
    private ArrayList<int[]> alBaseColors;          //base colors used
    private ArrayList<Integer> alColorList;         //random color list
    private ArrayList<Integer> alLightDark;         //light or dark of base colors
    private int[][] fullPalette;
    private Random rand = new Random();
    private boolean[] colorUsed;

    public PokePalette(){
        alBaseColors = new ArrayList<int[]>();
        alColorList = makeColorList();
        alLightDark = new ArrayList<Integer>();
        fullPalette = new int[16][3];
        makeNewColorUsed();
    }

    public PokePalette(ArrayList<int[]> albc, ArrayList<Integer> alcl,ArrayList<Integer> alld, boolean[] mcu){
        alBaseColors = albc;
        alColorList = alcl;
        alLightDark = alld;
        fullPalette = new int[16][3];
        colorUsed = mcu;
    }

    public PokePalette getClone(){
        makeNewColorUsed();
        return new PokePalette(alBaseColors,alColorList,alLightDark,colorUsed);
    }

    public ArrayList<int[]> getPalette(){
        return alBaseColors;
    }

    public ArrayList<Integer> getColorList(){
        return alColorList;
    }
    
    public ArrayList<Integer> getLightDark(){
        return alLightDark;
    }

    public int[][] getFullPalette(){
        return fullPalette;
    }

    public String getColor(int place){
        if (place >= 16 || place < 0){
            System.out.println("Error getting color with slot data of: "+place);
            return "";
        }
        if (!colorUsed[place]) return "null";
        return intColorToHex(fullPalette[place]);
    }

    public int[][] getEvenColors(int palSpots, int[] baseCol, boolean light, boolean dark, boolean rndcha, boolean endDarkened){
        return makeEvenColors(palSpots, baseCol, light, dark, rndcha, endDarkened);//randBaseColor(
    }

    public int[][] getOddColors(int palSpots, int[] baseCol, boolean light, boolean dark, boolean rndcha, boolean endDarkened){
        return makeOddColors(palSpots, baseCol, light, dark, rndcha, endDarkened);
    }

    public void setLight(int slot){
        if (slot >= alLightDark.size()){
            System.out.println(" >< ERROR! Light slot of " + slot + " does not exist! Max size is: " + alLightDark.size());
            return;
        }
        alLightDark.set(slot, LIGHT);
    }

    public void setDark(int slot){
        if (slot >= alLightDark.size()){
            System.out.println(" >< ERROR! Dark slot of " + slot + " does not exist! Max size is: " + alLightDark.size());
            return;
        }
        alLightDark.set(slot, DARK);
    }

    public void setRandLight(int slot){
        if (slot >= alLightDark.size()){
            System.out.println(" >< ERROR! Light slot of " + slot + " does not exist! Max size is: " + alLightDark.size());
            return;
        }
        alLightDark.set(slot, RAND_LIGHT);
    }

    public void setRandDark(int slot){
        if (slot >= alLightDark.size()){
            System.out.println(" >< ERROR! Dark slot of " + slot + " does not exist! Max size is: " + alLightDark.size());
            return;
        }
        alLightDark.set(slot, RAND_DARK);
    }

    public void ensureNotLight(int slot){
        if (slot >= alLightDark.size()){
            System.out.println(" >< ERROR! Slot of " + slot + " does not exist! Max size is: " + alLightDark.size());
            return;
        }
        if (alLightDark.get(slot) == LIGHT){
            alLightDark.set(slot, EMPTY); //it was light somehow(?)
        } else if (alLightDark.get(slot) == RAND_LIGHT){
            alLightDark.set(slot, RAND_DARK); //it was light by chance; make the chance switch to dark
        }
    }

    public void ensureNotDark(int slot){
        if (slot >= alLightDark.size()){
            System.out.println(" >< ERROR! Slot of " + slot + " does not exist! Max size is: " + alLightDark.size());
            return;
        }
        if (alLightDark.get(slot) == DARK){
            alLightDark.set(slot, EMPTY); //it was dark somehow(?)
        } else if (alLightDark.get(slot) == RAND_DARK){
            alLightDark.set(slot, RAND_LIGHT); //it was dark by chance; make the chance switch to light
        }
    }

    public void ensureBase(int slot){
        if (slot >= alLightDark.size()){
            System.out.println(" >< ERROR! Slot of " + slot + " does not exist! Max size is: " + alLightDark.size());
            return;
        }
        alLightDark.set(slot, EMPTY);
    }

    public void resetColors(){
        alBaseColors = new ArrayList<int[]>();
        alColorList = makeColorList();
        alLightDark = new ArrayList<Integer>();
        resetColorsFamily();
    }

    public void resetColorsFamily(){
        fullPalette = new int[16][3];
        for (int i=0;i<colorUsed.length;i++){
            colorUsed[i] = false;
        }
    }
    
    public void addRandomColorToBase(){
        int baseCol = alColorList.remove(rand.nextInt(alColorList.size()));
        alBaseColors.add(randBaseColor(baseColors[baseCol]));
        if (rand.nextInt(100) <= LIGHTDARK_CHANCE){
            if (rand.nextInt(2)==0) {
                alLightDark.add(RAND_DARK);
            }
            else alLightDark.add(RAND_LIGHT);
        } else{
            alLightDark.add(0);
        }
    }

    public void addTypeColorToBase(int type){
        int[] typeChoices = {type*3,type*3+1,type*3+2};
        int baseCol = rand.nextInt(3);

        int cycles = 0;
        for(;!alColorList.contains(typeChoices[baseCol%3]) && cycles<3;cycles++){
            baseCol = (baseCol+1)%3;
        }
        if (cycles >= 3){ //all type colors are already selected

        }
        baseCol = alColorList.remove(baseCol);

        alBaseColors.add(randBaseColor(baseColors[baseCol]));
        if (rand.nextInt(100) <= LIGHTDARK_CHANCE){
            if (rand.nextInt(2)==0) {
                alLightDark.add(RAND_DARK);
            }
            else alLightDark.add(RAND_LIGHT);
        } else{
            alLightDark.add(0);
        }
    }

    public void addTypeColorToBase(int type, int type2){
        int[] typeChoices = {type*3,type*3+1,type*3+2};
        int[] type2Choices = {type2*3,type2*3+1,type2*3+2};
        int baseCol = rand.nextInt(3);

        int cycles = 0;
        for(;!alColorList.contains(typeChoices[baseCol%3]) && cycles<3;cycles++){
            baseCol = (baseCol+1)%3;
        }
        if (cycles >= 3){ //all type 1 colors are already selected
            cycles = 0;
            baseCol = rand.nextInt(3);
            for(;!alColorList.contains(type2Choices[baseCol%3]) && cycles<3;cycles++){
                baseCol = (baseCol+1)%3;
            }
            if (cycles >= 3){ //all type 2 colors are already selected as well! get a random color
                baseCol = alColorList.remove(rand.nextInt(alColorList.size()));
            } else{ //we found a type 2 color
                baseCol = alColorList.remove(alColorList.indexOf(type2Choices[baseCol]));
            }
        } else{ //we found a type 1 color
            baseCol = alColorList.remove(alColorList.indexOf(typeChoices[baseCol]));
        }

        alBaseColors.add(randBaseColor(baseColors[baseCol]));
        if (rand.nextInt(100) <= LIGHTDARK_CHANCE){
            if (rand.nextInt(2)==0) {
                alLightDark.add(RAND_DARK);
            }
            else alLightDark.add(RAND_LIGHT);
        } else{
            alLightDark.add(0);
        }
    }

    public void fillBaseColorsByType(int colorTotal, int type1, int type2){
        while (alBaseColors.size() < colorTotal){
            if (alBaseColors.size() == 0){
                addTypeColorToBase(type1,type2);
            } else if(alBaseColors.size() == 1){
                addTypeColorToBase(type2,type1);
            }else{
                if (rand.nextInt(2) == 1){
                    addTypeColorToBase(type1,type2);
                } else{
                    addTypeColorToBase(type2,type1);
                }
            }
        }
    }

    public void fillBaseColors(int colorTotal){
        while (alBaseColors.size() < colorTotal){
            addRandomColorToBase();
        }
    }

    private void makeNewColorUsed(){
        colorUsed = new boolean[16];
        for (int i=0;i<16;i++){
            colorUsed[i] = false;
        }
    }

    //make a list of colors for pokemon; the list can be taken from to prevent duplicate colors
    private ArrayList<Integer> makeColorList(){
        ArrayList<Integer> out = new ArrayList<Integer>();
        for (int i=0;i<baseColors.length;i++){
            out.add(i);
        }
        return out;
    }

    //fill slots in 16-slot palette
    public void fillPaletteSlots(String info, int colorPtr, boolean endDarkened){
        if (alBaseColors.size() < colorPtr+1 || alLightDark.size() < colorPtr+1){
            System.out.println(" XXXXXXXX  - Not enough color data to fill palette slots!");
            return;
        }
        //get light/dark status
        boolean light,dark,rndcha;
        switch (alLightDark.get(colorPtr)){ //.remove(0)
            case DARK:
                light = false; dark = true; rndcha = false;
                break;
            case LIGHT:
                light = true; dark = false; rndcha = false;
                break;
            case RAND_LIGHT:
                light = false; dark = true; rndcha = true;
                break;
            case RAND_DARK:
                light = true; dark = false; rndcha = true;
                break;
            default:
                light = false; dark = false; rndcha = false;
        }

        //create colors to use
        String[] parts = info.split(",");
        int[][] shades;
        if (parts.length % 2 == 0){ // even
            shades = makeEvenColors(parts.length, alBaseColors.get(colorPtr), light, dark, rndcha, endDarkened); //remove(0)
        } else{ //odd
            shades = makeOddColors(parts.length, alBaseColors.get(colorPtr), light, dark, rndcha, endDarkened); //remove(0)
        }

        //get slots and fill them in as necessary
        for (int i=0;i<parts.length;i++){
            int slot = 0;
            try{
                slot = Integer.parseInt(parts[i])-1;
                colorUsed[slot] = true;
            } catch (NumberFormatException nfe){
                System.out.println("Could not parse color slot info into number: " + parts[i]);
            }
            for (int j=0;j<3;j++){ //fill R, G, and B
                fullPalette[slot][j] = shades[i][j];
            }
        }
    }

    //fill slots in 16-slot palette with aibling color data
    public void fillPaletteSlotsSibling(String info1, String info2, int sharedColor, int colorPtr, boolean endDarkened){
        if (alBaseColors.size() < colorPtr+1 || alLightDark.size() < colorPtr+1){
            System.out.println("Not enough color data to fill palette slots!");
            return;
        }
        int[] baseCol = alBaseColors.get(colorPtr);

        //get light/dark status
        boolean light,dark,rndcha;
        switch (alLightDark.get(colorPtr)){
            case DARK:
                light = false; dark = true; rndcha = false;
                break;
            case LIGHT:
                light = true; dark = false; rndcha = false;
                break;
            case RAND_DARK:
                light = false; dark = true; rndcha = true;
                break;
            case RAND_LIGHT:
                light = true; dark = false; rndcha = true;
                break;
            default:
                light = false; dark = false; rndcha = false;
        }

        //create colors to use
        String[] parts = info1.split(",");
        int[][] shades;
        if (parts.length % 2 == 0){ // even
            shades = makeEvenColors(parts.length, baseCol, light, dark, rndcha, endDarkened);
        } else{ //odd
            shades = makeOddColors(parts.length, baseCol, light, dark, rndcha, endDarkened);
        }

        //get slots and fill them in as necessary
        for (int i=0;i<parts.length;i++){
            int slot = 0;
            try{
                slot = Integer.parseInt(parts[i])-1;
                colorUsed[slot] = true;
            } catch (NumberFormatException nfe){
                System.out.println("Could not parse color slot info into number: " + parts[i]);
            }
            for (int j=0;j<3;j++){ //fill R, G, and B
                fullPalette[slot][j] = shades[i][j];
            }
        }
        //handle sibling palette
        int[] shColAr = fullPalette[sharedColor-1];
        if (shColAr[0] <= 3 && shColAr[1] <= 3 && shColAr[2] <= 3){
            for (int m=0;m<3;m++){
                shColAr[m] = (int)(baseCol[m] * 0.15);
            }
        } else if(shColAr[0] >= 252 && shColAr[1] >= 252 && shColAr[2] >= 252){
            for (int m=0;m<3;m++){
                shColAr[m] = (int)(baseCol[m] * 0.85);
            }
        }
        shades = makeSiblingPalette(shColAr, info2, sharedColor);
        
        parts = info2.split(",");
        //get slots and fill them in as necessary
        for (int i=0;i<parts.length;i++){
            int slot = 0;
            try{
                slot = Integer.parseInt(parts[i])-1;
                colorUsed[slot] = true;
            } catch (NumberFormatException nfe){
                System.out.println("Could not parse color slot info into number: " + parts[i]);
            }
            for (int j=0;j<3;j++){ //fill R, G, and B
                fullPalette[slot][j] = shades[i][j];
            }
        }
    }

    //makes a sibling palette for ; entries
    private int[][] makeSiblingPalette(int[] sharedCol,String colData,int shared){
        String[] places = colData.split(",");
        int sharedLoc = indexOf(places,shared+"");
        if (sharedLoc == -1){
            System.out.println("Error finding shared color in sibling palette.\n\t"+colData);
            return null;
        }
        int [][] out = new int[places.length][3];

        double[] variation = {
             (rand.nextInt(VARIA)+rand.nextInt(VARIA)+rand.nextInt(VARIA)+VARIABASE)*0.01*(Math.pow(-1,(rand.nextInt(2)))),
             (rand.nextInt(VARIA)+rand.nextInt(VARIA)+rand.nextInt(VARIA)+VARIABASE)*0.01*(Math.pow(-1,(rand.nextInt(2)))),
             (rand.nextInt(VARIA)+rand.nextInt(VARIA)+rand.nextInt(VARIA)+VARIABASE)*0.01*(Math.pow(-1,(rand.nextInt(2)))) };
        double[] leftShift = { getLeftColorChange(sharedLoc, sharedCol[0]),
                            getLeftColorChange(sharedLoc, sharedCol[1]),
                            getLeftColorChange(sharedLoc, sharedCol[2])};
        double[] rightShift = {getRightColorChange((places.length-sharedLoc-1), sharedCol[0]),
                            getRightColorChange((places.length-sharedLoc-1), sharedCol[1]),
                            getRightColorChange((places.length-sharedLoc-1), sharedCol[2])};

        //place shared color first
        out[sharedLoc] = sharedCol;

        //place color left and right of shared
        if (sharedLoc-1 >= 0){
            for (int p=0;p<3;p++){
                out[sharedLoc-1][p] = Math.min((int)(out[sharedLoc][p] + leftShift[p] *
                        (1+ variation[p])),255);
            }
        }
        if (sharedLoc+1 < out.length){
            for (int p=0;p<3;p++){
                out[sharedLoc+1][p] = Math.max((int)(out[sharedLoc][p] - rightShift[p] *
                        (1+ variation[p])),0);
            }
        }

        //fill colors left of shared
        for (int ptr = sharedLoc-2;ptr >= 0;ptr--){
            for (int p=0;p<3;p++){
                out[ptr][p] = Math.min((int)(out[ptr+1][p] + leftShift[p]),255);
            }
        }

        //fill colors right of shared
        for (int ptr = sharedLoc+2;ptr < out.length;ptr++){
            for (int p=0;p<3;p++){
                out[ptr][p] = Math.max((int)(out[ptr-1][p] - rightShift[p]),0);
            }
        }
        return out;
    }



    //randomizes a base color
    private int[] randBaseColor(int[] baseColor){
        int[] outColor = new int[3];

        //checks for valid color data
        if (baseColor.length != 3) {
            System.out.println("RGB values need all 3 values to make a color!");
            return null;
        }
        for (int i=0;i<3;i++){
            if (baseColor[i] > 255){
                System.out.println("Wrong color data, dummy! Max is 255!");
            }
        }

        //defining the color
        for (int i=0;i<3;i++){ //for each in R,G,B
            int randMin = (int)(-0.032 * baseColor[i] + 7.1);
            int randMax = (int)(-0.042 * baseColor[i] + 17.19);
            int change = rand.nextInt(randMax-randMin) + rand.nextInt(randMax);
            outColor[i] = Math.min(baseColor[i] +
                    (int)(baseColor[i] * change * 0.01 * Math.pow(-1, rand.nextInt(2))) ,255);
        }
//        System.out.println("randBaseColor = "+outColor[0]+" "+outColor[1]+" "+outColor[2]);
        return outColor;
    }

    private double getLeftColorChange(int palLimit, int maxRGB){
        if (palLimit == 0) return 0;
        double coeff = (rand.nextInt(5)+rand.nextInt(5)+rand.nextInt(5)+153)/(palLimit);
        return coeff - coeff/255 * maxRGB + palLimit * palLimit;
    }

    private double getRightColorChange(int palLimit, int maxRGB){
        if (palLimit == 0) return 0;
        double coeff = (rand.nextInt(5)+rand.nextInt(5)+rand.nextInt(5)+153)/(palLimit);
        return coeff - coeff/255 * (255-maxRGB) + palLimit * palLimit;
    }

    //makes palettes for even-numbered palettes
    private int[][] makeEvenColors(int palSpots, int[] baseCol, boolean light, boolean dark, boolean rndcha, boolean endDarkened){
        int[][] out = new int[palSpots][3];
        int[] baseColor = {baseCol[0],baseCol[1],baseCol[2]};

        double[] leftShift = {getLeftColorChange((palSpots)/2, baseCol[0]),
                            getLeftColorChange((palSpots)/2, baseCol[1]),
                            getLeftColorChange((palSpots)/2, baseCol[2])};
        double[] rightShift = {getRightColorChange((palSpots)/2, baseCol[0]),
                            getRightColorChange((palSpots)/2, baseCol[1]),
                            getRightColorChange((palSpots)/2, baseCol[2])};

        if (light || dark){
            if (dark){
                double mod;
                if (rndcha){
                    mod = 0.2;
                } else {
                    mod = 0.5;
                }
                for (int i=0;i<3;i++){
                    baseColor[i] = Math.max((int)(baseColor[i] - rightShift[i] * (palSpots/2+1)*mod),0);
                }
                leftShift[0] = getRightColorChange((palSpots)/2, baseColor[0]);
                leftShift[1] = getRightColorChange((palSpots)/2, baseColor[1]);
                leftShift[2] = getRightColorChange((palSpots)/2, baseColor[2]);
                rightShift[0] = getRightColorChange((palSpots)/2, baseColor[0]);
                rightShift[1] = getRightColorChange((palSpots)/2, baseColor[1]);
                rightShift[2] = getRightColorChange((palSpots)/2, baseColor[2]);
            } else { //light
                double mod = 0.5;
                if (rndcha){
                    mod = 0.2;
                } else {
                    mod = 0.5;
                }
                //lightenBaseCoeff(baseCol);
                /*int[] bc = baseColor.clone();
                Arrays.sort(bc);
                int THRES1 = 200;
                int THRES2 = 225;
                if (bc[1] > THRES2 && bc[2] > THRES2){
                    mod = 0.0;
                }else if (bc[1] > THRES1 && bc[2] > THRES1){
                    mod = 0.25;
                }*/

                for (int i=0;i<3;i++){
                    baseColor[i] = Math.min((int)(baseColor[i] + leftShift[i] * (palSpots/2+1)*mod),255);
                }
                leftShift[0] = getLeftColorChange((palSpots)/2, baseColor[0]);
                leftShift[1] = getLeftColorChange((palSpots)/2, baseColor[1]);
                leftShift[2] = getLeftColorChange((palSpots)/2, baseColor[2]);
                rightShift[0] = getLeftColorChange((palSpots)/2, baseColor[0]);
                rightShift[1] = getLeftColorChange((palSpots)/2, baseColor[1]);
                rightShift[2] = getLeftColorChange((palSpots)/2, baseColor[2]);

                mod = lightDarkenCoeff(baseCol);
                for (int k=0;k<rightShift.length;k++){
                    rightShift[k] += mod * getLeftColorChange((palSpots)/2, baseColor[k]);
                }
                
                /*if (bc[1] > THRES2 && bc[2] > THRES2){
                        for (int k=0;k<rightShift.length;k++){
                            rightShift[k] += 0.6 * getRightColorChange((palSpots)/2, baseColor[k]);
                        }

                }else if (bc[1] > THRES1 && bc[2] > THRES1){
                        for (int k=0;k<rightShift.length;k++){
                            rightShift[k] += 0.2 * getRightColorChange((palSpots)/2, baseColor[k]);
                        }
                }*/

            }
        }

        //set base colors in the middle
        for (int totalChange = 0,r=0;totalChange < TOTAL_CHANGE_THRESHOLD && r<5; r++){
            totalChange = 0;
            for (int p=0;p<3;p++){
                out[out.length/2-1][p] = Math.min((int)(baseColor[p] + leftShift[p] * 0.5),255);
                out[out.length/2][p] = Math.max((int)(baseColor[p] - rightShift[p] * 0.5),0);
                totalChange += (out[out.length/2][p] >> 2) -
                                (out[out.length/2-1][p] >> 2);
            }
        }

        //make all colors left of middle
        for (int ptr = out.length/2-2;ptr >= 0;ptr--){
            for (int p=0;p<3;p++){
                out[ptr][p] = Math.min((int)(out[ptr+1][p] + leftShift[p]),255);
            }
        }
        //make all colors right of middle
        for (int ptr = out.length/2+1;ptr < out.length;ptr++){
            for (int p=0;p<3;p++){
                out[ptr][p] = Math.max((int)(out[ptr-1][p] - rightShift[p]),0);
            }
        }

        //end darkened
        if (endDarkened){
            int ptr = out.length-1;
            for (int j=0;j<1;j++){ //can be adjusted to darken more
                for (int p=0;p<3;p++){
                    out[ptr][p] = Math.max((int)(out[ptr][p] - rightShift[p]),0);
                }
            }
        }
        return out;
    }

    private double lightDarkenCoeff(int[] baseCol){
        int[] bc = baseCol.clone();
        Arrays.sort(bc);
        double avg = (bc[1] + bc[2])/2.0;
        if (baseCol[1]  >= 225 && baseCol[0] < 200 && baseCol[2] < 200) avg += 75; //45
        if (baseCol[1]  >= 225) avg += 75;
        if (avg > 255) avg = 255;
        if (avg < 192) {
            return 0.5;
        } else{
            return (0.0002 * Math.pow(avg-192,3.5)+50)*0.01;
        }
        //return (0.000000000002 * (Math.pow(avg, 5.647)))*0.01;
    }

    private double lightenBaseCoeff(int[] baseCol){
        int[] bc = baseCol.clone();
        Arrays.sort(bc);
        double avg = (bc[1] + bc[2])/2.0;
        return (0.0000000000000002 * (Math.pow(avg, 6.8)))*0.01;
    }

    //makes palettes for odd-numbered palettes
    private int[][] makeOddColors(int palSpots, int[] baseCol, boolean light, boolean dark, boolean rndcha, boolean endDarkened){
        int[][] out = new int[palSpots][3];
        int[] baseColor = {baseCol[0],baseCol[1],baseCol[2]};
        double[] leftShift = {getLeftColorChange((palSpots-1)/2, baseColor[0]),
                            getLeftColorChange((palSpots-1)/2, baseColor[1]),
                            getLeftColorChange((palSpots-1)/2, baseColor[2])};
        double[] rightShift = {getRightColorChange((palSpots-1)/2, baseColor[0]),
                            getRightColorChange((palSpots-1)/2, baseColor[1]),
                            getRightColorChange((palSpots-1)/2, baseColor[2])};

        if (light || dark){
            if (dark){
                double mod;
                if (rndcha){
                    mod = 0.2;
                } else{
                    mod = 0.5;
                }
                for (int i=0;i<3;i++){
                    baseColor[i] = Math.max((int)(baseColor[i] - rightShift[i] * (palSpots/2+1)*mod),0);
                }
                leftShift[0] = getRightColorChange((palSpots-1)/2, baseColor[0]);
                leftShift[1] = getRightColorChange((palSpots-1)/2, baseColor[1]);
                leftShift[2] = getRightColorChange((palSpots-1)/2, baseColor[2]);
                rightShift[0] = getRightColorChange((palSpots-1)/2, baseColor[0]);
                rightShift[1] = getRightColorChange((palSpots-1)/2, baseColor[1]);
                rightShift[2] = getRightColorChange((palSpots-1)/2, baseColor[2]);
            } else{ //light
                double mod;
                if (rndcha){
                    mod = 0.2;
                } else{
                    mod = 0.5;
                }
                //lightenBaseCoeff(baseCol);
                /*int[] bc = baseColor.clone();
                Arrays.sort(bc);
                int THRES1 = 200;
                int THRES2 = 225;
                if (bc[1] > THRES2 && bc[2] > THRES2){
                    mod = 0.0;
                }else if (bc[1] > THRES1 && bc[2] > THRES1){
                    mod = 0.25;
                }*/

                for (int i=0;i<3;i++){
                    baseColor[i] = Math.min((int)(baseColor[i] + leftShift[i] * (palSpots/2+1)*mod),255);
                }

                leftShift[0] = getLeftColorChange((palSpots-1)/2, baseColor[0]);
                leftShift[1] = getLeftColorChange((palSpots-1)/2, baseColor[1]);
                leftShift[2] = getLeftColorChange((palSpots-1)/2, baseColor[2]);
                rightShift[0] = getLeftColorChange((palSpots-1)/2, baseColor[0]);
                rightShift[1] = getLeftColorChange((palSpots-1)/2, baseColor[1]);
                rightShift[2] = getLeftColorChange((palSpots-1)/2, baseColor[2]);

                /*if (bc[1] > THRES2 && bc[2] > THRES2){
                        for (int k=0;k<rightShift.length;k++){
                            rightShift[k] += 0.6 * getRightColorChange((palSpots)/2, baseColor[k]);
                        }

                }else if (bc[1] > THRES1 && bc[2] > THRES1){
                        for (int k=0;k<rightShift.length;k++){
                            rightShift[k] += 0.2 * getRightColorChange((palSpots)/2, baseColor[k]);
                        }
                }*/

                mod = lightDarkenCoeff(baseCol);
                for (int k=0;k<rightShift.length;k++){
                    rightShift[k] += mod * getLeftColorChange((palSpots)/2, baseColor[k]);
                }

                /*int[] bc = baseColor.clone();
                Arrays.sort(bc);
                int THRES = 255;
                if (bc[1] > THRES && bc[2] > THRES){
                    for (int k=0;k<rightShift.length;k++){
                        rightShift[k] *= 2;
                    }
                }*/
            }
        }

        //set base color in the middle
        try{
            out[out.length/2] = baseColor;
        } catch (ArrayIndexOutOfBoundsException aioobe){
            if (out.length != 1) System.out.println("Error with odd palette middle slot array index");
            out[0] = baseColor;
            return out;
        }

        //make all colors left of middle
        int[] col;
        for (int ptr = out.length/2-1;ptr >= 0;ptr--){
            for (int p=0;p<3;p++){
                out[ptr][p] = Math.min((int)(out[ptr+1][p] + leftShift[p]),255);
            }
        }

        //make all colors right of middle
        for (int ptr = out.length/2+1;ptr < out.length;ptr++){
            for (int p=0;p<3;p++){
                out[ptr][p] = Math.max((int)(out[ptr-1][p] - rightShift[p]),0);
            }
        }

        //end darkened
        if (endDarkened){
            int ptr = out.length-1;
            for (int j=0;j<1;j++){ //can be adjusted to darken more
                for (int p=0;p<3;p++){
                    out[ptr][p] = Math.max((int)(out[ptr][p] - rightShift[p]),0);
                }
            }
        }
        return out;
    }
    
    //translates a RGB color to GBA Hex
    private String intColorToHex(int[] col){
        if (col.length != 3){
            System.out.println("Color to Hex error!");
            return "";
        }
        int color = ((col[2] >>3) << 10) + ((col[1] >> 3) << 5) + (col[0] >> 3);
        String out = intToHex(color,4);
        return (out.substring(2) + out.substring(0,2));
    }

    public void print(){
         for (int k=0;k<fullPalette.length;k++){
             if (colorUsed[k]){
                System.out.print(intColorToHex(fullPalette[k])+ " ");
             } else {
                 System.out.print("---- ");
             }
        } System.out.println("");
    }

    public void printColorUsed(){
        for (int i=0;i<colorUsed.length;i++){
            System.out.print(colorUsed[i] + " ");
        }System.out.println("");
    }

     private int indexOf(Object[] ar, Object src){
         for (int i=0;i<ar.length;i++){
             if (ar[i].equals(src)) return i;
         } return -1;
     }

     private int indexOf(int[] ar, int src){
         for (int i=0;i<ar.length;i++){
             if (ar[i]==src) return i;
         } return -1;
     }

     private String intToHex(int n, int size){
         StringBuffer out = new StringBuffer(java.lang.Integer.toHexString(n));
         for (int i=size-out.length();i>0;i--){
             out.insert(0,"0");
         }
         if (out.length() > size){
             System.out.println("intToHex error: int " + n + " too big for size " + size);
             return out.toString().substring(out.length()-size);
         }
         return out.toString();
     }


    public final int[][] baseColors = {{255,157,231},{203,149,253},{197,154,139},   //NRM
                                    {164,106,68},{201,101,111},{235,166,90},        //FTG
                                    {119,183,183},{113,165,251},{142,89,64},        //FLY
                                    {236,13,215},{36,255,36},{151,135,184},         //PSN
                                    {216,172,124},{168,162,119},{211,163,90},       //GRD
                                    {146,182,133},{108,120,140},{146,129,103},      //RCK
                                    {159,240,79},{149,173,63},{234,234,8},          //BUG
                                    {135,41,250},{66,68,138},{77,95,157},           //GHO
                                    {192,192,192},{228,135,31},{226,82,33},         //STL
                                    {168,139,19},{236,23,103},{126,12,203},         //---
                                    {253,130,47},{205,10,10},{250,213,43},          //FIR
                                    {64,69,252},{10,170,170},{97,209,165},          //WAT
                                    {9,183,9},{78,145,49},{214,193,50},             //GRS
                                    {251,242,89},{32,251,251},{250,24,24},          //ELE
                                    {197,77,242},{206,96,159},{130,48,184},         //PSY
                                    {130,252,230},{196,208,210},{122,186,250},      //ICE
                                    {216,61,65},{140,53,53},{53,64,189},            //DRG
                                    {74,74,74},{9,9,187},{146,12,12}  };            //DRK
}
