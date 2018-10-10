/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package emeraldrandomizer;

/**
 *
 * @author Artemis251
 */
public class backup {

    /*
     //gets a percentage for color manipulation; lr = left/right, 1/-1
    private double getPerc(int colData, int lr){
        /*if (colData < COLOR_THRESHOLDS[0]){
            return getRandPerc(COLOR_CHANGE[2+2*lr][0], COLOR_CHANGE[2+2*lr][1]);
        } else if (colData < COLOR_THRESHOLDS[1]){
            return getRandPerc(COLOR_CHANGE[2+lr][0], COLOR_CHANGE[2+lr][1]);
        } else if (colData < COLOR_THRESHOLDS[2]){
            return getRandPerc(COLOR_CHANGE[2][0], COLOR_CHANGE[2][1]);
        } else if (colData < COLOR_THRESHOLDS[3]){
            return getRandPerc(COLOR_CHANGE[2-lr][0], COLOR_CHANGE[2-lr][1]);
        } else {
            return getRandPerc(COLOR_CHANGE[2-2*lr][0], COLOR_CHANGE[2-2*lr][1]);
        }
        return getPerc(colData);
    }

    //gets a percentage for color manipulation
    private double getPerc(int colData){
        if (colData < COLOR_THRESHOLDS[0]){
            return getRandPerc(COLOR_CHANGE[0][0], COLOR_CHANGE[0][1]);
        } else if (colData < COLOR_THRESHOLDS[1]){
            return getRandPerc(COLOR_CHANGE[1][0], COLOR_CHANGE[1][1]);
        } else if (colData < COLOR_THRESHOLDS[2]){
            return getRandPerc(COLOR_CHANGE[2][0], COLOR_CHANGE[2][1]);
        } else if (colData < COLOR_THRESHOLDS[3]){
            return getRandPerc(COLOR_CHANGE[3][0], COLOR_CHANGE[3][1]);
        } else {
            return getRandPerc(COLOR_CHANGE[4][0], COLOR_CHANGE[4][1]);
        }
    }

    //random percent for changing colors
    private double getRandPerc(int min, int max){
        Random rand = new Random();
        int out = 0;
        for (int i=0;i<3;i++){
            out += rand.nextInt(max-min)+min;
        }
        return out * 0.01;
    }

    //gets the rank of a color based on its value
    private int getColorRank(int color){
        int rank = 0;
        for (;rank<=COLOR_THRESHOLDS2.length && color > COLOR_THRESHOLDS2[rank];rank++){ }
        return rank;
    }

    //ensure the colors change enough. lr = left or right, 1 or -1.
    private void ensureColorChange(int[] out, int[] old, double booster, int lr,int size){
        //int[] out = {in[0],in[1],in[2]};
        /*if (!(out[0] > 230 || out[1] > 230 || out[2] > 230)){
            return;
        }
        String rank = getColorRank(out[0])+""+getColorRank(out[1])+""+getColorRank(out[2]);
        int reps = 0;
        double CHANGE = 0.55;
        int SIZE_UPPER = 4;
        for (int p=1;p<5;p++){
            if (rank.contains("0") && rank.contains((COLOR_THRESHOLDS2.length-p)+"")){
                String len = (COLOR_THRESHOLDS2.length-p)+"";
                if (rank.indexOf("0") != rank.lastIndexOf("0")){ //0,0,255
                    reps = 4-p  + (int)(size/SIZE_UPPER*CHANGE);
                    break;
                } else if (rank.indexOf(len) != rank.lastIndexOf(len)){ //0,255,255
                    reps = 5-p + (int)(size/SIZE_UPPER*CHANGE);
                    break;
                } else{ //0,255,other
                    break;
                }
            }
        }
        double perc;
        /*while (((lr*out[0] - lr*old[0]*1.0)/old[0]
              + (lr*out[1] - lr*old[1]*1.0)/old[1]
              + (lr*out[2] - lr*old[2]*1.0)/old[2]) < 60){//AMOUNT_DIFF){
        double[] percs = {getPerc(old[0],lr), getPerc(old[1],lr), getPerc(old[2],lr)};
        for(;reps > 0;reps--){
            for (int p=0;p<old.length;p++){
                perc = getPerc(old[p],lr);
                out[p] = Math.max(Math.min((int)(out[p] + out[p] * percs[p] * booster * lr * CHANGE),255),0);
            }
        }
        return;// out;
    }

    //makes palettes for even-numbered palettes
    private int[][] makeEvenColors(int palSpots, int[] baseCol, boolean light){
        int[][] out = new int[palSpots][3];
        int[] baseColor = {baseCol[0],baseCol[1],baseCol[2]};
        double booster = 1.5 - (palSpots-1)/3 * 0.5;    //makes colors less extreme for more palSpots
        if (!light){
            for (int t=0;t<1;t++){ //baseColor[0]+baseColor[1]+baseColor[2] > DARKEN_THRESHOLD
                for (int k=0;k<3;k++){ //for all RGB colors
                        baseColor[k] = baseColor[k]*2/3;
                }
            }
        }

        //set base colors in the middle
        int[] tempcolor = new int[3];
        for (int p=0;p<baseColor.length;p++){
            double perc = getPerc(baseColor[p],1);
            tempcolor[p] = Math.min((int)(baseColor[p] + baseColor[p] * perc * booster),255);
        }
        ensureColorChange(tempcolor,baseColor,booster,1,palSpots);
        for (int p=0;p<baseColor.length;p++){
            out[out.length/2-1][p] = (int)(baseColor[p] + (tempcolor[p] - baseColor[p]) * 0.5);
        }
        for (int p=0;p<baseColor.length;p++){
            double perc = getPerc(baseColor[p],-1);
            tempcolor[p] = Math.max((int)(baseColor[p] - baseColor[p] * perc * booster),0);
        }
        ensureColorChange(tempcolor,baseColor,booster,-1,palSpots);
        for (int p=0;p<baseColor.length;p++){
            out[out.length/2][p] = (int)(baseColor[p] - (baseColor[p] - tempcolor[p]) * 0.5);
        }

        /*for (int p=0;p<baseColor.length;p++){ //for all R,G,B
            double perc = getPerc(baseColor[p],1);
            out[out.length/2-1][p] = Math.min((int)(baseColor[p] + baseColor[p] * perc * booster * 0.5),255);
            perc = getPerc(baseColor[p],-1);
            out[out.length/2][p] = Math.max((int)(baseColor[p] - baseColor[p] * perc * booster * 0.5),0);
//            System.out.println("  perc: "+perc+"   -  mid: "+out[out.length/2][p]);
        }




        //make all colors left of middle
        int[] col;
        boolean done = false;
        for (int ptr = out.length/2-2;ptr >= 0;ptr--){
            col = out[ptr + 1];
            double perc = 1;

            for (int p=0;p<col.length;p++){
                perc = getPerc(col[p],1);
                out[ptr][p] = Math.min((int)(col[p] + col[p] * perc * booster),255);
            }
            ensureColorChange(out[ptr],out[ptr+1],booster,1,palSpots);
        }
        //make all colors right of middle
        for (int ptr = out.length/2+1;ptr < out.length;ptr++){
            col = out[ptr - 1];
            double perc = 1;
            for (int p=0;p<col.length;p++){
                perc = getPerc(col[p],-1);
                out[ptr][p] = Math.max((int)(col[p] - col[p] * perc * booster),0);
            }
            ensureColorChange(out[ptr],out[ptr-1],booster,-1,palSpots);
        }
        /*if (!light){
            for (int t=0;out[out.length-1][0]+out[out.length-1][1]+out[out.length-1][2] > DARKEN_THRESHOLD;t++){
                for (int k=0;k<3;k++){ //for all RGB colors
                    for (int h=0;h<=t && h<out.length-1;h++){ //increase how far back you go for each instance
                        out[out.length-1-h][k] = out[out.length-1-h][k]*2/3;
                    }
                }
            }
        }
        return out;
    }

    //makes palettes for odd-numbered palettes
    private int[][] makeOddColors(int palSpots, int[] baseCol, boolean light){
        int[][] out = new int[palSpots][3];
        int[] baseColor = {baseCol[0],baseCol[1],baseCol[2]};
        double booster = 1.5 - (palSpots-2)/3 * 0.5;    //makes colors less extreme for more palSpots

        if (!light){
            for (int t=0;baseColor[0]+baseColor[1]+baseColor[2] > DARKEN_THRESHOLD;t++){
                for (int k=0;k<3;k++){ //for all RGB colors
                        baseColor[k] = baseColor[k]*2/3;
                }
            }
        }

        //set base color in the middle
        try{
            out[out.length/2] = baseColor;
            System.out.println("baseColor - " + baseColor[0] + " " + baseColor[1] + " " + baseColor[2]);
        } catch (ArrayIndexOutOfBoundsException aioobe){
            if (out.length != 1) System.out.println("Error with odd palette middle slot array index");
            out[0] = baseColor;
            return out;
        }

        //make all colors left of middle
        int[] col;
        for (int ptr = out.length/2-1;ptr >= 0;ptr--){
            col = out[ptr + 1];
            double perc;
            for (int p=0;p<col.length;p++){
                perc = getPerc(col[p],1);
                out[ptr][p] = Math.min((int)(col[p] + col[p] * perc * booster),255);
            }
            ensureColorChange(out[ptr],out[ptr+1],booster,1,palSpots);
        }
        //make all colors right of middle
        for (int ptr = out.length/2+1;ptr < out.length;ptr++){
            col = out[ptr - 1];
            double perc;
            for (int p=0;p<col.length;p++){
                perc = getPerc(col[p],-1);
                out[ptr][p] = Math.max((int)(col[p] - col[p] * perc * booster),0);
            }
            ensureColorChange(out[ptr],out[ptr-1],booster,-1,palSpots);
        }

        /*if (!light){
            //until the last color's r+g+b is < darken_thresh
            for (int t=0;out[out.length-1][0]+out[out.length-1][1]+out[out.length-1][2] > DARKEN_THRESHOLD;t++){
                for (int k=0;k<3;k++){ //for all RGB colors
                    for (int h=0;h<=t && h<out.length;h++){ //increase how far back you go for each instance
                        out[out.length-1-h][k] = out[out.length-1-h][k]*2/3; //darken color
                    }
                }
            }
        }*/
//        return out;
//    }
}
