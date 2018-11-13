package emeraldrandomizer;

import javafx.geometry.Pos;
import javafx.scene.Scene;

import java.io.File;

import javafx.event.ActionEvent;
import javafx.event.EventHandler;
import javafx.geometry.HPos;
import javafx.geometry.Insets;
import javafx.scene.control.*;
import javafx.scene.layout.*;
import javafx.scene.text.Font;
import javafx.scene.text.FontWeight;
import javafx.stage.FileChooser;
import javafx.stage.Stage;

public class EmeraldRandomizerGUI {
	
	public static final int padding = 20;
	//File picking constants
	private static final FileChooser.ExtensionFilter ROMExtFilter = new FileChooser.ExtensionFilter("GBA ROM", "*.gba");
	private static final FileChooser fileChooser = new FileChooser();	
	private static File lastPath = new File(System.getProperty("user.dir"));
	private static RomData romData = null;
	private Stage stage = null;
	
	public EmeraldRandomizerGUI(Stage s)
	{
		stage = s;
	}
	
	public Scene getScene(int width, int height)
	{
		return new Scene(constructGrid(), width, height);
	}
	
	private GridPane constructGrid()
	{
		GridPane grid = new GridPane();
		grid.setAlignment(Pos.TOP_LEFT);
		grid.setPadding(new Insets(padding,padding,padding,padding));
		//grid.add(new Label("WOot"), 0, 0);
        final Button openButton = new Button("Open ROM");
        openButton.setOnAction(
            new EventHandler<ActionEvent>() {
                @Override
                public void handle(final ActionEvent e) {
                	openROM();
                }
            });
		grid.add(openButton, 0, 3);
		return grid;
	}
	//Open the file selector to locate and open a ROM
	private void openROM()
	{
 		fileChooser.getExtensionFilters().clear();
		fileChooser.getExtensionFilters().add(ROMExtFilter);
    	fileChooser.setInitialDirectory(lastPath);
        File file = fileChooser.showOpenDialog(stage);
        if (file != null) {
        	romData = new RomLoader().openROM(file.getAbsolutePath());
        	lastPath = file.getParentFile();
        }
	}

}
