package emeraldrandomizer;

import javafx.application.Application;
import javafx.stage.Stage;

public class EmeraldRandomizerApp extends Application {
	public static void main(String[] args)
	{
		launch(args);
	}
	
	@Override
    public void init() throws Exception {
        super.init();
        System.out.println("Initialize");
    }
	
	@Override
    public void start(Stage primaryStage) throws Exception {
		EmeraldRandomizerGUI GUI = new EmeraldRandomizerGUI();
        primaryStage.setTitle("Pokemon Emerald Randomizer");
        primaryStage.setScene(GUI.getScene(1280, 720));
        primaryStage.show();
    }
	
	@Override
	public void stop() throws Exception {
		super.stop();
		System.out.println("Cleanup");
	}
}
