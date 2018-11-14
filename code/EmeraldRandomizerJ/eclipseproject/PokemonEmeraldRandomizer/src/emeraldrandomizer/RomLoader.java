package emeraldrandomizer;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.security.MessageDigest;

import javafx.scene.control.Alert;
import javafx.scene.control.Alert.AlertType;

public class RomLoader {
    private static final Alert alert = new Alert(AlertType.NONE);
	private static String romPath = "";
    private static byte[] rom;

	public RomData openROM(String path)
	{
		romPath = path;
        try
        {
        	FileInputStream ist = new FileInputStream(romPath);
            rom = new byte[ist.available()];
            ist.read(rom,0,ist.available());
            ist.close();
            System.out.println(rom);
            checkHash();
            return new RomData();
        }catch (java.lang.StringIndexOutOfBoundsException jsioobe){
        	alert.setContentText("Error Reading File!");
        } catch (NullPointerException npe){
            alert.setContentText("No filename given!\n"+npe);
        } catch (FileNotFoundException fnfe){
            alert.setContentText("ROM could not be read!");
        } catch (IOException ioe){
            alert.setContentText("I/O problem! : "+ioe);
        }
    	alert.setAlertType(AlertType.ERROR);
   	 	alert.showAndWait();
        return null;
    }
    private void checkHash(){
        try{
            MessageDigest digest = MessageDigest.getInstance("MD5");
            digest.update(rom, 0, rom.length);
            String hash = new java.math.BigInteger(1,digest.digest()).toString(16);
            if (!hash.equals("7b058a7aea5bfbb352026727ebd87e17")){
            	alert.setAlertType(AlertType.WARNING);
            	alert.setContentText("WARNING: The base ROM does not match the target ROM "+
                        "this program was intended for!\n\nMD5: " + hash + "\nExpected: "+
                        "7b058a7aea5bfbb352026727ebd87e17" + "\nInvalid base ROM!");
           	 	alert.showAndWait();
            }
        } catch (java.security.NoSuchAlgorithmException nsae){
        	alert.setAlertType(AlertType.ERROR);
        	alert.setContentText("Error loading Hash type!" + "\nHash Error");
       	 	alert.showAndWait();
        }
    }

}
