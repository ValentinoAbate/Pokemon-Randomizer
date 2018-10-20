package emeraldrandomizer;

import javafx.geometry.Pos;
import javafx.scene.Scene;
import javafx.event.ActionEvent;
import javafx.event.EventHandler;
import javafx.geometry.HPos;
import javafx.geometry.Insets;
import javafx.scene.control.*;
import javafx.scene.layout.*;
import javafx.scene.text.Font;
import javafx.scene.text.FontWeight;

public class EmeraldRandomizerGUI {
	
	public static final int padding = 20;
	
	public Scene getScene(int width, int height)
	{
		return new Scene(constructGrid(), width, height);
	}
	
	private GridPane constructGrid()
	{
		GridPane grid = new GridPane();
		grid.setAlignment(Pos.TOP_LEFT);
		grid.setPadding(new Insets(padding,padding,padding,padding));
		grid.add(new Label("WOot"), 0, 0);
		grid.add(new Label("WOot"), 0, 1);
		grid.add(new Label("WOot"), 0, 2);
		return grid;
	}

}
