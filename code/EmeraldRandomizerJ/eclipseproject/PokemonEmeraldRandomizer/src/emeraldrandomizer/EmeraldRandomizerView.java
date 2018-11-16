/*
 * Pokemon Emerald Randomizer, v.2.2
 * Date of release: 13 April 2014
 * Author: Artemis251
 * Thanks for takin' a peek!
 */

package emeraldrandomizer;

import java.awt.Color;
import java.awt.Graphics2D;
import java.awt.Rectangle;
import java.awt.image.BufferedImage;
import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.PrintWriter;
import java.security.MessageDigest;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Random;
import javax.imageio.ImageIO;
import javax.swing.DefaultComboBoxModel;
import org.jdesktop.application.Action;
import org.jdesktop.application.SingleFrameApplication;
import org.jdesktop.application.FrameView;
import javax.swing.JDialog;
import javax.swing.JFileChooser;
import javax.swing.JFrame;
import javax.swing.JOptionPane;
import org.jdesktop.application.Application.ExitListener;

public class EmeraldRandomizerView extends FrameView {

    public EmeraldRandomizerView(SingleFrameApplication app) {
        super(app);
        initComponents();
        setActive(false);
        unsaved(false);

        app.addExitListener(new ExitListener(){
            public boolean canExit(java.util.EventObject e) {
                return miCloseActionPerformed2();
            }
            public void willExit(java.util.EventObject event) { }
        });
    }

    @Action
    public void showAboutBox() {
        if (aboutBox == null) {
            JFrame mainFrame = EmeraldRandomizerAppOld.getApplication().getMainFrame();
            aboutBox = new EmeraldRandomizerAboutBox(mainFrame);
            aboutBox.setLocationRelativeTo(mainFrame);
        }
        EmeraldRandomizerAppOld.getApplication().show(aboutBox);
    }

    @SuppressWarnings("unchecked")
    // <editor-fold defaultstate="collapsed" desc="Generated Code">//GEN-BEGIN:initComponents
    private void initComponents() {

        mainPanel = new javax.swing.JPanel();
        pStarter = new javax.swing.JPanel();
        lStarters = new javax.swing.JLabel();
        cbStarter1 = new javax.swing.JComboBox();
        cbItem = new javax.swing.JComboBox();
        jLabel1 = new javax.swing.JLabel();
        jLabel2 = new javax.swing.JLabel();
        cbStarter2 = new javax.swing.JComboBox();
        jSeparator3 = new javax.swing.JSeparator();
        jLabel4 = new javax.swing.JLabel();
        cbStarter3 = new javax.swing.JComboBox();
        jLabel6 = new javax.swing.JLabel();
        cbEnsureBasic = new javax.swing.JCheckBox();
        bRandomStarters = new javax.swing.JButton();
        cbNoLegends = new javax.swing.JCheckBox();
        jPanel1 = new javax.swing.JPanel();
        jLabel7 = new javax.swing.JLabel();
        jPanel2 = new javax.swing.JPanel();
        rbRandom = new javax.swing.JRadioButton();
        rbUnchanged = new javax.swing.JRadioButton();
        rbGlobal = new javax.swing.JRadioButton();
        rbSubs = new javax.swing.JRadioButton();
        cbUnique = new javax.swing.JCheckBox();
        cbUniqueLegends = new javax.swing.JCheckBox();
        cbNoLegendsWild = new javax.swing.JCheckBox();
        cbKeepForm = new javax.swing.JCheckBox();
        cbEnsureAll = new javax.swing.JCheckBox();
        cbRandTrades = new javax.swing.JCheckBox();
        jPanel3 = new javax.swing.JPanel();
        jPanel4 = new javax.swing.JPanel();
        rbTrainerUnchanged = new javax.swing.JRadioButton();
        rbTrainerGlobal = new javax.swing.JRadioButton();
        rbTrainerRandom = new javax.swing.JRadioButton();
        jLabel3 = new javax.swing.JLabel();
        cbRivals = new javax.swing.JCheckBox();
        cbTrainerNames = new javax.swing.JCheckBox();
        cbRandTrainerHeldItems = new javax.swing.JCheckBox();
        cbRandTrainerUseItems = new javax.swing.JCheckBox();
        cbNoTrainerLegends = new javax.swing.JCheckBox();
        cbBattleFrontier = new javax.swing.JCheckBox();
        bOpen = new javax.swing.JButton();
        bSave = new javax.swing.JButton();
        bPrint = new javax.swing.JButton();
        jPanel5 = new javax.swing.JPanel();
        jLabel5 = new javax.swing.JLabel();
        cbMovesets = new javax.swing.JCheckBox();
        cbRandTMLearn = new javax.swing.JCheckBox();
        cbTMs = new javax.swing.JCheckBox();
        cbRandStats = new javax.swing.JCheckBox();
        cbRandTypes = new javax.swing.JCheckBox();
        cbRandAbilities = new javax.swing.JCheckBox();
        cbRandHeldItems = new javax.swing.JCheckBox();
        cbEnsureAttacks = new javax.swing.JCheckBox();
        cbMetronome = new javax.swing.JCheckBox();
        jSeparator2 = new javax.swing.JSeparator();
        jPanel6 = new javax.swing.JPanel();
        jLabel8 = new javax.swing.JLabel();
        cbKeepColors = new javax.swing.JCheckBox();
        jPanel7 = new javax.swing.JPanel();
        rbPalNoChange = new javax.swing.JRadioButton();
        rbPalType = new javax.swing.JRadioButton();
        rbPalRandom = new javax.swing.JRadioButton();
        cbPalPrimary = new javax.swing.JCheckBox();
        cbShinyNorm = new javax.swing.JCheckBox();
        jScrollPane1 = new javax.swing.JScrollPane();
        taWild = new javax.swing.JTextArea();
        jPanel8 = new javax.swing.JPanel();
        jLabel9 = new javax.swing.JLabel();
        cbFixEvos = new javax.swing.JCheckBox();
        cbHeartScale = new javax.swing.JCheckBox();
        cbRandPickup = new javax.swing.JCheckBox();
        cbRandFieldItems = new javax.swing.JCheckBox();
        cbMultTMs = new javax.swing.JCheckBox();
        cbNatlDex = new javax.swing.JCheckBox();
        menuBar = new javax.swing.JMenuBar();
        javax.swing.JMenu fileMenu = new javax.swing.JMenu();
        miOpen = new javax.swing.JMenuItem();
        miSave = new javax.swing.JMenuItem();
        miSaveAs = new javax.swing.JMenuItem();
        miClose = new javax.swing.JMenuItem();
        jSeparator1 = new javax.swing.JPopupMenu.Separator();
        javax.swing.JMenuItem miExit = new javax.swing.JMenuItem();
        mQuickSettings = new javax.swing.JMenu();
        miLow = new javax.swing.JMenuItem();
        miMedium = new javax.swing.JMenuItem();
        miHigh = new javax.swing.JMenuItem();
        miArty = new javax.swing.JMenuItem();
        miRandom = new javax.swing.JMenuItem();
        jSeparator4 = new javax.swing.JPopupMenu.Separator();
        miClear = new javax.swing.JMenuItem();
        javax.swing.JMenu helpMenu = new javax.swing.JMenu();
        javax.swing.JMenuItem aboutMenuItem = new javax.swing.JMenuItem();
        bgWildPokemon = new javax.swing.ButtonGroup();
        bgTrainer = new javax.swing.ButtonGroup();
        bgPalette = new javax.swing.ButtonGroup();

        mainPanel.setName("mainPanel"); // NOI18N

        pStarter.setBorder(javax.swing.BorderFactory.createEtchedBorder());
        pStarter.setName("pStarter"); // NOI18N
        pStarter.setPreferredSize(new java.awt.Dimension(250, 199));

        org.jdesktop.application.ResourceMap resourceMap = org.jdesktop.application.Application.getInstance(emeraldrandomizer.EmeraldRandomizerAppOld.class).getContext().getResourceMap(EmeraldRandomizerView.class);
        lStarters.setText(resourceMap.getString("lStarters.text")); // NOI18N
        lStarters.setName("lStarters"); // NOI18N

        cbStarter1.setModel(new javax.swing.DefaultComboBoxModel(new String[] { "---" }));
        cbStarter1.setName("cbStarter1"); // NOI18N
        cbStarter1.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbStarter1ItemStateChanged(evt);
            }
        });

        cbItem.setModel(new javax.swing.DefaultComboBoxModel(new String[] { "---" }));
        cbItem.setName("cbItem"); // NOI18N
        cbItem.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbItemItemStateChanged(evt);
            }
        });

        jLabel1.setText(resourceMap.getString("jLabel1.text")); // NOI18N
        jLabel1.setName("jLabel1"); // NOI18N

        jLabel2.setText(resourceMap.getString("jLabel2.text")); // NOI18N
        jLabel2.setName("jLabel2"); // NOI18N

        cbStarter2.setModel(new javax.swing.DefaultComboBoxModel(new String[] { "---" }));
        cbStarter2.setName("cbStarter2"); // NOI18N
        cbStarter2.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbStarter2ItemStateChanged(evt);
            }
        });

        jSeparator3.setName("jSeparator3"); // NOI18N

        jLabel4.setText(resourceMap.getString("jLabel4.text")); // NOI18N
        jLabel4.setName("jLabel4"); // NOI18N

        cbStarter3.setModel(new javax.swing.DefaultComboBoxModel(new String[] { "---" }));
        cbStarter3.setName("cbStarter3"); // NOI18N
        cbStarter3.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbStarter3ItemStateChanged(evt);
            }
        });

        jLabel6.setText(resourceMap.getString("jLabel6.text")); // NOI18N
        jLabel6.setName("jLabel6"); // NOI18N

        cbEnsureBasic.setText(resourceMap.getString("cbEnsureBasic.text")); // NOI18N
        cbEnsureBasic.setName("cbEnsureBasic"); // NOI18N
        cbEnsureBasic.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbEnsureBasicItemStateChanged(evt);
            }
        });

        bRandomStarters.setText(resourceMap.getString("bRandomStarters.text")); // NOI18N
        bRandomStarters.setName("bRandomStarters"); // NOI18N
        bRandomStarters.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                bRandomStartersActionPerformed(evt);
            }
        });

        cbNoLegends.setText(resourceMap.getString("cbNoLegends.text")); // NOI18N
        cbNoLegends.setName("cbNoLegends"); // NOI18N

        javax.swing.GroupLayout pStarterLayout = new javax.swing.GroupLayout(pStarter);
        pStarter.setLayout(pStarterLayout);
        pStarterLayout.setHorizontalGroup(
            pStarterLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(pStarterLayout.createSequentialGroup()
                .addGroup(pStarterLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
                    .addGroup(pStarterLayout.createSequentialGroup()
                        .addContainerGap()
                        .addGroup(pStarterLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
                            .addComponent(lStarters, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)
                            .addGroup(pStarterLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.TRAILING, false)
                                .addGroup(javax.swing.GroupLayout.Alignment.LEADING, pStarterLayout.createSequentialGroup()
                                    .addComponent(jLabel2)
                                    .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                                    .addComponent(cbItem, 0, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE))
                                .addComponent(jSeparator3, javax.swing.GroupLayout.Alignment.LEADING)
                                .addGroup(javax.swing.GroupLayout.Alignment.LEADING, pStarterLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
                                    .addGroup(javax.swing.GroupLayout.Alignment.TRAILING, pStarterLayout.createSequentialGroup()
                                        .addComponent(jLabel1)
                                        .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                                        .addComponent(cbStarter1, javax.swing.GroupLayout.PREFERRED_SIZE, 142, javax.swing.GroupLayout.PREFERRED_SIZE))
                                    .addGroup(javax.swing.GroupLayout.Alignment.TRAILING, pStarterLayout.createSequentialGroup()
                                        .addComponent(jLabel4)
                                        .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                                        .addComponent(cbStarter2, javax.swing.GroupLayout.PREFERRED_SIZE, 142, javax.swing.GroupLayout.PREFERRED_SIZE))
                                    .addGroup(javax.swing.GroupLayout.Alignment.TRAILING, pStarterLayout.createSequentialGroup()
                                        .addComponent(jLabel6)
                                        .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                                        .addComponent(cbStarter3, javax.swing.GroupLayout.PREFERRED_SIZE, 142, javax.swing.GroupLayout.PREFERRED_SIZE)))
                                .addComponent(cbEnsureBasic, javax.swing.GroupLayout.Alignment.LEADING)
                                .addComponent(cbNoLegends, javax.swing.GroupLayout.Alignment.LEADING))))
                    .addGroup(javax.swing.GroupLayout.Alignment.TRAILING, pStarterLayout.createSequentialGroup()
                        .addGap(9, 9, 9)
                        .addComponent(bRandomStarters, javax.swing.GroupLayout.DEFAULT_SIZE, 203, Short.MAX_VALUE)))
                .addContainerGap())
        );
        pStarterLayout.setVerticalGroup(
            pStarterLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(pStarterLayout.createSequentialGroup()
                .addComponent(lStarters, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addGroup(pStarterLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE)
                    .addComponent(cbStarter1, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)
                    .addComponent(jLabel1))
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addGroup(pStarterLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE)
                    .addComponent(cbStarter2, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)
                    .addComponent(jLabel4))
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addGroup(pStarterLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE)
                    .addComponent(cbStarter3, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)
                    .addComponent(jLabel6))
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(jSeparator3, javax.swing.GroupLayout.PREFERRED_SIZE, 10, javax.swing.GroupLayout.PREFERRED_SIZE)
                .addGap(1, 1, 1)
                .addGroup(pStarterLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE)
                    .addComponent(cbItem, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)
                    .addComponent(jLabel2))
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                .addComponent(cbEnsureBasic)
                .addGap(3, 3, 3)
                .addComponent(cbNoLegends)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(bRandomStarters)
                .addGap(39, 39, 39))
        );

        jPanel1.setBorder(javax.swing.BorderFactory.createEtchedBorder());
        jPanel1.setName("jPanel1"); // NOI18N

        jLabel7.setText(resourceMap.getString("jLabel7.text")); // NOI18N
        jLabel7.setName("jLabel7"); // NOI18N

        jPanel2.setBorder(javax.swing.BorderFactory.createEtchedBorder());
        jPanel2.setName("jPanel2"); // NOI18N

        bgWildPokemon.add(rbRandom);
        rbRandom.setText(resourceMap.getString("rbRandom.text")); // NOI18N
        rbRandom.setName("rbRandom"); // NOI18N
        rbRandom.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                rbRandomItemStateChanged(evt);
            }
        });

        bgWildPokemon.add(rbUnchanged);
        rbUnchanged.setSelected(true);
        rbUnchanged.setText(resourceMap.getString("rbUnchanged.text")); // NOI18N
        rbUnchanged.setName("rbUnchanged"); // NOI18N
        rbUnchanged.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                rbUnchangedItemStateChanged(evt);
            }
        });

        bgWildPokemon.add(rbGlobal);
        rbGlobal.setText(resourceMap.getString("rbGlobal.text")); // NOI18N
        rbGlobal.setName("rbGlobal"); // NOI18N
        rbGlobal.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                rbGlobalItemStateChanged(evt);
            }
        });

        bgWildPokemon.add(rbSubs);
        rbSubs.setText(resourceMap.getString("rbSubs.text")); // NOI18N
        rbSubs.setName("rbSubs"); // NOI18N
        rbSubs.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                rbSubsItemStateChanged(evt);
            }
        });

        javax.swing.GroupLayout jPanel2Layout = new javax.swing.GroupLayout(jPanel2);
        jPanel2.setLayout(jPanel2Layout);
        jPanel2Layout.setHorizontalGroup(
            jPanel2Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(jPanel2Layout.createSequentialGroup()
                .addContainerGap(javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                .addGroup(jPanel2Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING, false)
                    .addComponent(rbRandom)
                    .addComponent(rbSubs, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                    .addComponent(rbGlobal)
                    .addComponent(rbUnchanged, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)))
        );
        jPanel2Layout.setVerticalGroup(
            jPanel2Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(jPanel2Layout.createSequentialGroup()
                .addContainerGap()
                .addComponent(rbUnchanged)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(rbGlobal)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(rbSubs)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(rbRandom, javax.swing.GroupLayout.PREFERRED_SIZE, 23, javax.swing.GroupLayout.PREFERRED_SIZE)
                .addContainerGap(javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE))
        );

        cbUnique.setText(resourceMap.getString("cbUnique.text")); // NOI18N
        cbUnique.setName("cbUnique"); // NOI18N
        cbUnique.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbUniqueItemStateChanged(evt);
            }
        });

        cbUniqueLegends.setText(resourceMap.getString("cbUniqueLegends.text")); // NOI18N
        cbUniqueLegends.setName("cbUniqueLegends"); // NOI18N
        cbUniqueLegends.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbUniqueLegendsItemStateChanged(evt);
            }
        });

        cbNoLegendsWild.setText(resourceMap.getString("cbNoLegendsWild.text")); // NOI18N
        cbNoLegendsWild.setName("cbNoLegendsWild"); // NOI18N
        cbNoLegendsWild.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbNoLegendsWildItemStateChanged(evt);
            }
        });

        cbKeepForm.setText(resourceMap.getString("cbKeepForm.text")); // NOI18N
        cbKeepForm.setName("cbKeepForm"); // NOI18N
        cbKeepForm.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbKeepFormItemStateChanged(evt);
            }
        });

        cbEnsureAll.setText(resourceMap.getString("cbEnsureAll.text")); // NOI18N
        cbEnsureAll.setName("cbEnsureAll"); // NOI18N
        cbEnsureAll.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbEnsureAllItemStateChanged(evt);
            }
        });

        cbRandTrades.setText(resourceMap.getString("cbRandTrades.text")); // NOI18N
        cbRandTrades.setName("cbRandTrades"); // NOI18N
        cbRandTrades.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbRandTradesItemStateChanged(evt);
            }
        });

        javax.swing.GroupLayout jPanel1Layout = new javax.swing.GroupLayout(jPanel1);
        jPanel1.setLayout(jPanel1Layout);
        jPanel1Layout.setHorizontalGroup(
            jPanel1Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(jPanel1Layout.createSequentialGroup()
                .addContainerGap()
                .addGroup(jPanel1Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
                    .addComponent(cbRandTrades)
                    .addComponent(cbEnsureAll)
                    .addComponent(cbNoLegendsWild)
                    .addComponent(jPanel2, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)
                    .addComponent(jLabel7, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)
                    .addGroup(jPanel1Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.TRAILING)
                        .addComponent(cbUniqueLegends)
                        .addComponent(cbUnique))
                    .addComponent(cbKeepForm))
                .addContainerGap(javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE))
        );
        jPanel1Layout.setVerticalGroup(
            jPanel1Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(jPanel1Layout.createSequentialGroup()
                .addComponent(jLabel7, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(jPanel2, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbUnique)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbUniqueLegends)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbEnsureAll)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbKeepForm)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbNoLegendsWild)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbRandTrades)
                .addContainerGap(javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE))
        );

        jPanel3.setBorder(javax.swing.BorderFactory.createEtchedBorder());
        jPanel3.setName("jPanel3"); // NOI18N

        jPanel4.setBorder(javax.swing.BorderFactory.createEtchedBorder());
        jPanel4.setName("jPanel4"); // NOI18N

        bgTrainer.add(rbTrainerUnchanged);
        rbTrainerUnchanged.setSelected(true);
        rbTrainerUnchanged.setText(resourceMap.getString("rbTrainerUnchanged.text")); // NOI18N
        rbTrainerUnchanged.setName("rbTrainerUnchanged"); // NOI18N
        rbTrainerUnchanged.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                rbTrainerUnchangedItemStateChanged(evt);
            }
        });

        bgTrainer.add(rbTrainerGlobal);
        rbTrainerGlobal.setText(resourceMap.getString("rbTrainerGlobal.text")); // NOI18N
        rbTrainerGlobal.setName("rbTrainerGlobal"); // NOI18N
        rbTrainerGlobal.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                rbTrainerGlobalItemStateChanged(evt);
            }
        });

        bgTrainer.add(rbTrainerRandom);
        rbTrainerRandom.setText(resourceMap.getString("rbTrainerRandom.text")); // NOI18N
        rbTrainerRandom.setName("rbTrainerRandom"); // NOI18N
        rbTrainerRandom.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                rbTrainerRandomItemStateChanged(evt);
            }
        });

        javax.swing.GroupLayout jPanel4Layout = new javax.swing.GroupLayout(jPanel4);
        jPanel4.setLayout(jPanel4Layout);
        jPanel4Layout.setHorizontalGroup(
            jPanel4Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(jPanel4Layout.createSequentialGroup()
                .addContainerGap()
                .addGroup(jPanel4Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
                    .addGroup(jPanel4Layout.createSequentialGroup()
                        .addComponent(rbTrainerUnchanged, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                        .addGap(89, 89, 89))
                    .addGroup(jPanel4Layout.createSequentialGroup()
                        .addComponent(rbTrainerGlobal)
                        .addContainerGap())
                    .addGroup(jPanel4Layout.createSequentialGroup()
                        .addComponent(rbTrainerRandom)
                        .addContainerGap(47, Short.MAX_VALUE))))
        );
        jPanel4Layout.setVerticalGroup(
            jPanel4Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(jPanel4Layout.createSequentialGroup()
                .addContainerGap()
                .addComponent(rbTrainerUnchanged)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(rbTrainerGlobal)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(rbTrainerRandom, javax.swing.GroupLayout.PREFERRED_SIZE, 23, javax.swing.GroupLayout.PREFERRED_SIZE)
                .addContainerGap(javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE))
        );

        jLabel3.setText(resourceMap.getString("jLabel3.text")); // NOI18N
        jLabel3.setName("jLabel3"); // NOI18N

        cbRivals.setText(resourceMap.getString("cbRivals.text")); // NOI18N
        cbRivals.setName("cbRivals"); // NOI18N
        cbRivals.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbRivalsItemStateChanged(evt);
            }
        });

        cbTrainerNames.setText(resourceMap.getString("cbTrainerNames.text")); // NOI18N
        cbTrainerNames.setName("cbTrainerNames"); // NOI18N
        cbTrainerNames.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbTrainerNamesItemStateChanged(evt);
            }
        });

        cbRandTrainerHeldItems.setText(resourceMap.getString("cbRandTrainerHeldItems.text")); // NOI18N
        cbRandTrainerHeldItems.setActionCommand(resourceMap.getString("cbRandTrainerHeldItems.actionCommand")); // NOI18N
        cbRandTrainerHeldItems.setName("cbRandTrainerHeldItems"); // NOI18N
        cbRandTrainerHeldItems.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbRandTrainerHeldItemsItemStateChanged(evt);
            }
        });

        cbRandTrainerUseItems.setText(resourceMap.getString("cbRandTrainerUseItems.text")); // NOI18N
        cbRandTrainerUseItems.setName("cbRandTrainerUseItems"); // NOI18N
        cbRandTrainerUseItems.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbRandTrainerUseItemsItemStateChanged(evt);
            }
        });

        cbNoTrainerLegends.setText(resourceMap.getString("cbNoTrainerLegends.text")); // NOI18N
        cbNoTrainerLegends.setName("cbNoTrainerLegends"); // NOI18N
        cbNoTrainerLegends.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbNoTrainerLegendsItemStateChanged(evt);
            }
        });

        cbBattleFrontier.setText(resourceMap.getString("cbBattleFrontier.text")); // NOI18N
        cbBattleFrontier.setName("cbBattleFrontier"); // NOI18N
        cbBattleFrontier.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbBattleFrontierItemStateChanged(evt);
            }
        });

        javax.swing.GroupLayout jPanel3Layout = new javax.swing.GroupLayout(jPanel3);
        jPanel3.setLayout(jPanel3Layout);
        jPanel3Layout.setHorizontalGroup(
            jPanel3Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(jPanel3Layout.createSequentialGroup()
                .addContainerGap()
                .addGroup(jPanel3Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
                    .addComponent(cbBattleFrontier)
                    .addComponent(cbNoTrainerLegends)
                    .addComponent(cbRivals)
                    .addComponent(cbRandTrainerUseItems)
                    .addComponent(cbRandTrainerHeldItems)
                    .addComponent(jPanel4, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)
                    .addComponent(jLabel3, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)
                    .addComponent(cbTrainerNames))
                .addContainerGap(javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE))
        );
        jPanel3Layout.setVerticalGroup(
            jPanel3Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(jPanel3Layout.createSequentialGroup()
                .addComponent(jLabel3, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(jPanel4, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbTrainerNames)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbRandTrainerHeldItems)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbRandTrainerUseItems)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbRivals)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbNoTrainerLegends)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbBattleFrontier)
                .addContainerGap(30, Short.MAX_VALUE))
        );

        bOpen.setText(resourceMap.getString("bOpen.text")); // NOI18N
        bOpen.setName("bOpen"); // NOI18N
        bOpen.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                bOpenActionPerformed(evt);
            }
        });

        bSave.setText(resourceMap.getString("bSave.text")); // NOI18N
        bSave.setName("bSave"); // NOI18N
        bSave.setPreferredSize(new java.awt.Dimension(85, 23));
        bSave.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                bSaveActionPerformed(evt);
            }
        });

        bPrint.setText(resourceMap.getString("bPrint.text")); // NOI18N
        bPrint.setName("bPrint"); // NOI18N
        bPrint.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                bPrintActionPerformed(evt);
            }
        });

        jPanel5.setBorder(javax.swing.BorderFactory.createEtchedBorder());
        jPanel5.setName("jPanel5"); // NOI18N

        jLabel5.setText(resourceMap.getString("jLabel5.text")); // NOI18N
        jLabel5.setName("jLabel5"); // NOI18N

        cbMovesets.setText(resourceMap.getString("cbMovesets.text")); // NOI18N
        cbMovesets.setName("cbMovesets"); // NOI18N
        cbMovesets.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbMovesetsItemStateChanged(evt);
            }
        });

        cbRandTMLearn.setText(resourceMap.getString("cbRandTMLearn.text")); // NOI18N
        cbRandTMLearn.setName("cbRandTMLearn"); // NOI18N
        cbRandTMLearn.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbRandTMLearnItemStateChanged(evt);
            }
        });

        cbTMs.setText(resourceMap.getString("cbTMs.text")); // NOI18N
        cbTMs.setName("cbTMs"); // NOI18N
        cbTMs.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbTMsItemStateChanged(evt);
            }
        });

        cbRandStats.setText(resourceMap.getString("cbRandStats.text")); // NOI18N
        cbRandStats.setName("cbRandStats"); // NOI18N
        cbRandStats.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbRandStatsItemStateChanged(evt);
            }
        });

        cbRandTypes.setText(resourceMap.getString("cbRandTypes.text")); // NOI18N
        cbRandTypes.setName("cbRandTypes"); // NOI18N
        cbRandTypes.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbRandTypesItemStateChanged(evt);
            }
        });

        cbRandAbilities.setText(resourceMap.getString("cbRandAbilities.text")); // NOI18N
        cbRandAbilities.setName("cbRandAbilities"); // NOI18N
        cbRandAbilities.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbRandAbilitiesItemStateChanged(evt);
            }
        });

        cbRandHeldItems.setText(resourceMap.getString("cbRandHeldItems.text")); // NOI18N
        cbRandHeldItems.setName("cbRandHeldItems"); // NOI18N
        cbRandHeldItems.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbRandHeldItemsItemStateChanged(evt);
            }
        });

        cbEnsureAttacks.setText(resourceMap.getString("cbEnsureAttacks.text")); // NOI18N
        cbEnsureAttacks.setName("cbEnsureAttacks"); // NOI18N
        cbEnsureAttacks.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbEnsureAttacksItemStateChanged(evt);
            }
        });

        cbMetronome.setText(resourceMap.getString("cbMetronome.text")); // NOI18N
        cbMetronome.setName("cbMetronome"); // NOI18N
        cbMetronome.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbMetronomeItemStateChanged(evt);
            }
        });

        jSeparator2.setName("jSeparator2"); // NOI18N

        javax.swing.GroupLayout jPanel5Layout = new javax.swing.GroupLayout(jPanel5);
        jPanel5.setLayout(jPanel5Layout);
        jPanel5Layout.setHorizontalGroup(
            jPanel5Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(jPanel5Layout.createSequentialGroup()
                .addGroup(jPanel5Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
                    .addGroup(jPanel5Layout.createSequentialGroup()
                        .addContainerGap()
                        .addGroup(jPanel5Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
                            .addComponent(cbTMs)
                            .addComponent(cbRandHeldItems)
                            .addComponent(cbRandAbilities)
                            .addComponent(cbRandTypes)
                            .addComponent(cbRandStats)
                            .addComponent(jLabel5, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)
                            .addComponent(jSeparator2, javax.swing.GroupLayout.PREFERRED_SIZE, 171, javax.swing.GroupLayout.PREFERRED_SIZE)))
                    .addGroup(jPanel5Layout.createSequentialGroup()
                        .addGap(10, 10, 10)
                        .addGroup(jPanel5Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
                            .addComponent(cbMetronome)
                            .addComponent(cbEnsureAttacks)
                            .addComponent(cbMovesets)
                            .addComponent(cbRandTMLearn, javax.swing.GroupLayout.DEFAULT_SIZE, 206, Short.MAX_VALUE))))
                .addContainerGap())
        );
        jPanel5Layout.setVerticalGroup(
            jPanel5Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(jPanel5Layout.createSequentialGroup()
                .addComponent(jLabel5, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED)
                .addComponent(cbRandStats)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbRandTypes)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbRandAbilities)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbRandHeldItems)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(jSeparator2, javax.swing.GroupLayout.PREFERRED_SIZE, 10, javax.swing.GroupLayout.PREFERRED_SIZE)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbTMs)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbRandTMLearn)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbMovesets)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbEnsureAttacks)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbMetronome)
                .addContainerGap(javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE))
        );

        jPanel6.setBorder(javax.swing.BorderFactory.createEtchedBorder());
        jPanel6.setName("jPanel6"); // NOI18N

        jLabel8.setText(resourceMap.getString("jLabel8.text")); // NOI18N
        jLabel8.setName("jLabel8"); // NOI18N

        cbKeepColors.setText(resourceMap.getString("cbKeepColors.text")); // NOI18N
        cbKeepColors.setName("cbKeepColors"); // NOI18N
        cbKeepColors.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbKeepColorsItemStateChanged(evt);
            }
        });

        jPanel7.setBorder(javax.swing.BorderFactory.createEtchedBorder());
        jPanel7.setName("jPanel7"); // NOI18N

        bgPalette.add(rbPalNoChange);
        rbPalNoChange.setSelected(true);
        rbPalNoChange.setText(resourceMap.getString("rbPalNoChange.text")); // NOI18N
        rbPalNoChange.setName("rbPalNoChange"); // NOI18N
        rbPalNoChange.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                rbPalNoChangeItemStateChanged(evt);
            }
        });

        bgPalette.add(rbPalType);
        rbPalType.setText(resourceMap.getString("rbPalType.text")); // NOI18N
        rbPalType.setName("rbPalType"); // NOI18N
        rbPalType.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                rbPalTypeItemStateChanged(evt);
            }
        });

        bgPalette.add(rbPalRandom);
        rbPalRandom.setText(resourceMap.getString("rbPalRandom.text")); // NOI18N
        rbPalRandom.setName("rbPalRandom"); // NOI18N
        rbPalRandom.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                rbPalRandomItemStateChanged(evt);
            }
        });

        javax.swing.GroupLayout jPanel7Layout = new javax.swing.GroupLayout(jPanel7);
        jPanel7.setLayout(jPanel7Layout);
        jPanel7Layout.setHorizontalGroup(
            jPanel7Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(jPanel7Layout.createSequentialGroup()
                .addContainerGap()
                .addGroup(jPanel7Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
                    .addComponent(rbPalNoChange)
                    .addComponent(rbPalType)
                    .addComponent(rbPalRandom))
                .addContainerGap(28, Short.MAX_VALUE))
        );
        jPanel7Layout.setVerticalGroup(
            jPanel7Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(jPanel7Layout.createSequentialGroup()
                .addContainerGap()
                .addComponent(rbPalNoChange)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(rbPalType)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(rbPalRandom)
                .addContainerGap(javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE))
        );

        cbPalPrimary.setText(resourceMap.getString("cbPalPrimary.text")); // NOI18N
        cbPalPrimary.setName("cbPalPrimary"); // NOI18N
        cbPalPrimary.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbPalPrimaryItemStateChanged(evt);
            }
        });

        cbShinyNorm.setText(resourceMap.getString("cbShinyNorm.text")); // NOI18N
        cbShinyNorm.setName("cbShinyNorm"); // NOI18N
        cbShinyNorm.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbShinyNormItemStateChanged(evt);
            }
        });

        javax.swing.GroupLayout jPanel6Layout = new javax.swing.GroupLayout(jPanel6);
        jPanel6.setLayout(jPanel6Layout);
        jPanel6Layout.setHorizontalGroup(
            jPanel6Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(jPanel6Layout.createSequentialGroup()
                .addGroup(jPanel6Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
                    .addGroup(jPanel6Layout.createSequentialGroup()
                        .addGap(10, 10, 10)
                        .addGroup(jPanel6Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
                            .addComponent(cbShinyNorm, javax.swing.GroupLayout.DEFAULT_SIZE, 189, Short.MAX_VALUE)
                            .addComponent(cbKeepColors, javax.swing.GroupLayout.DEFAULT_SIZE, 189, Short.MAX_VALUE)))
                    .addGroup(jPanel6Layout.createSequentialGroup()
                        .addContainerGap()
                        .addGroup(jPanel6Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.TRAILING, false)
                            .addComponent(jPanel7, javax.swing.GroupLayout.Alignment.LEADING, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                            .addComponent(cbPalPrimary, javax.swing.GroupLayout.Alignment.LEADING, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                            .addComponent(jLabel8, javax.swing.GroupLayout.Alignment.LEADING, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE))
                        .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED, 14, Short.MAX_VALUE)))
                .addContainerGap())
        );
        jPanel6Layout.setVerticalGroup(
            jPanel6Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(jPanel6Layout.createSequentialGroup()
                .addComponent(jLabel8, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(jPanel7, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbPalPrimary)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbKeepColors)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbShinyNorm)
                .addContainerGap(9, Short.MAX_VALUE))
        );

        jScrollPane1.setName("jScrollPane1"); // NOI18N

        taWild.setColumns(20);
        taWild.setEditable(false);
        taWild.setRows(5);
        taWild.setText(resourceMap.getString("taWild.text")); // NOI18N
        taWild.setName("taWild"); // NOI18N
        jScrollPane1.setViewportView(taWild);

        jPanel8.setBorder(javax.swing.BorderFactory.createEtchedBorder());
        jPanel8.setName("jPanel8"); // NOI18N

        jLabel9.setText(resourceMap.getString("jLabel9.text")); // NOI18N
        jLabel9.setName("jLabel9"); // NOI18N

        cbFixEvos.setText(resourceMap.getString("cbFixEvos.text")); // NOI18N
        cbFixEvos.setName("cbFixEvos"); // NOI18N
        cbFixEvos.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbFixEvosItemStateChanged(evt);
            }
        });

        cbHeartScale.setText(resourceMap.getString("cbHeartScale.text")); // NOI18N
        cbHeartScale.setName("cbHeartScale"); // NOI18N
        cbHeartScale.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbHeartScaleItemStateChanged(evt);
            }
        });

        cbRandPickup.setText(resourceMap.getString("cbRandPickup.text")); // NOI18N
        cbRandPickup.setName("cbRandPickup"); // NOI18N
        cbRandPickup.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbRandPickupItemStateChanged(evt);
            }
        });

        cbRandFieldItems.setText(resourceMap.getString("cbRandFieldItems.text")); // NOI18N
        cbRandFieldItems.setName("cbRandFieldItems"); // NOI18N
        cbRandFieldItems.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbRandFieldItemsItemStateChanged(evt);
            }
        });

        cbMultTMs.setText(resourceMap.getString("cbMultTMs.text")); // NOI18N
        cbMultTMs.setName("cbMultTMs"); // NOI18N
        cbMultTMs.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbMultTMsItemStateChanged(evt);
            }
        });

        cbNatlDex.setText(resourceMap.getString("cbNatlDex.text")); // NOI18N
        cbNatlDex.setName("cbNatlDex"); // NOI18N
        cbNatlDex.addItemListener(new java.awt.event.ItemListener() {
            public void itemStateChanged(java.awt.event.ItemEvent evt) {
                cbNatlDexItemStateChanged(evt);
            }
        });

        javax.swing.GroupLayout jPanel8Layout = new javax.swing.GroupLayout(jPanel8);
        jPanel8.setLayout(jPanel8Layout);
        jPanel8Layout.setHorizontalGroup(
            jPanel8Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(jPanel8Layout.createSequentialGroup()
                .addContainerGap()
                .addGroup(jPanel8Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
                    .addComponent(cbNatlDex)
                    .addComponent(cbMultTMs)
                    .addComponent(cbRandFieldItems)
                    .addComponent(cbRandPickup)
                    .addComponent(cbHeartScale)
                    .addComponent(cbFixEvos)
                    .addComponent(jLabel9, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE))
                .addContainerGap(10, Short.MAX_VALUE))
        );
        jPanel8Layout.setVerticalGroup(
            jPanel8Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(jPanel8Layout.createSequentialGroup()
                .addComponent(jLabel9, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED)
                .addComponent(cbFixEvos)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbHeartScale)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbRandPickup)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbRandFieldItems)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbMultTMs)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(cbNatlDex)
                .addContainerGap(28, Short.MAX_VALUE))
        );

        javax.swing.GroupLayout mainPanelLayout = new javax.swing.GroupLayout(mainPanel);
        mainPanel.setLayout(mainPanelLayout);
        mainPanelLayout.setHorizontalGroup(
            mainPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(mainPanelLayout.createSequentialGroup()
                .addContainerGap()
                .addGroup(mainPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.TRAILING, false)
                    .addComponent(jScrollPane1, javax.swing.GroupLayout.Alignment.LEADING)
                    .addGroup(javax.swing.GroupLayout.Alignment.LEADING, mainPanelLayout.createSequentialGroup()
                        .addComponent(bOpen, javax.swing.GroupLayout.PREFERRED_SIZE, 194, javax.swing.GroupLayout.PREFERRED_SIZE)
                        .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                        .addComponent(bSave, javax.swing.GroupLayout.PREFERRED_SIZE, 198, javax.swing.GroupLayout.PREFERRED_SIZE)
                        .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                        .addComponent(bPrint, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE))
                    .addGroup(javax.swing.GroupLayout.Alignment.LEADING, mainPanelLayout.createSequentialGroup()
                        .addGroup(mainPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING, false)
                            .addComponent(jPanel5, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                            .addComponent(pStarter, javax.swing.GroupLayout.DEFAULT_SIZE, 226, Short.MAX_VALUE))
                        .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                        .addGroup(mainPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
                            .addComponent(jPanel6, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                            .addComponent(jPanel1, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE))
                        .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                        .addGroup(mainPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
                            .addComponent(jPanel8, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)
                            .addComponent(jPanel3, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE))))
                .addGap(572, 572, 572))
        );
        mainPanelLayout.setVerticalGroup(
            mainPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(javax.swing.GroupLayout.Alignment.TRAILING, mainPanelLayout.createSequentialGroup()
                .addContainerGap()
                .addGroup(mainPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE)
                    .addComponent(bOpen, javax.swing.GroupLayout.PREFERRED_SIZE, 33, javax.swing.GroupLayout.PREFERRED_SIZE)
                    .addComponent(bSave, javax.swing.GroupLayout.PREFERRED_SIZE, 33, javax.swing.GroupLayout.PREFERRED_SIZE)
                    .addComponent(bPrint, javax.swing.GroupLayout.DEFAULT_SIZE, 33, Short.MAX_VALUE))
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addGroup(mainPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
                    .addGroup(mainPanelLayout.createSequentialGroup()
                        .addGroup(mainPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING, false)
                            .addComponent(jPanel3, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                            .addComponent(jPanel1, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE))
                        .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                        .addGroup(mainPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING, false)
                            .addComponent(jPanel8, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                            .addComponent(jPanel6, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)))
                    .addGroup(mainPanelLayout.createSequentialGroup()
                        .addComponent(pStarter, javax.swing.GroupLayout.PREFERRED_SIZE, 219, javax.swing.GroupLayout.PREFERRED_SIZE)
                        .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                        .addComponent(jPanel5, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)))
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(jScrollPane1, javax.swing.GroupLayout.PREFERRED_SIZE, 97, javax.swing.GroupLayout.PREFERRED_SIZE)
                .addGap(9, 9, 9))
        );

        menuBar.setName("menuBar"); // NOI18N

        fileMenu.setText(resourceMap.getString("fileMenu.text")); // NOI18N
        fileMenu.setName("fileMenu"); // NOI18N

        miOpen.setAccelerator(javax.swing.KeyStroke.getKeyStroke(java.awt.event.KeyEvent.VK_O, java.awt.event.InputEvent.CTRL_MASK));
        miOpen.setText(resourceMap.getString("miOpen.text")); // NOI18N
        miOpen.setName("miOpen"); // NOI18N
        miOpen.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                miOpenActionPerformed(evt);
            }
        });
        fileMenu.add(miOpen);

        miSave.setAccelerator(javax.swing.KeyStroke.getKeyStroke(java.awt.event.KeyEvent.VK_S, java.awt.event.InputEvent.CTRL_MASK));
        miSave.setText(resourceMap.getString("miSave.text")); // NOI18N
        miSave.setName("miSave"); // NOI18N
        miSave.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                miSaveActionPerformed(evt);
            }
        });
        fileMenu.add(miSave);

        miSaveAs.setText(resourceMap.getString("miSaveAs.text")); // NOI18N
        miSaveAs.setName("miSaveAs"); // NOI18N
        miSaveAs.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                miSaveAsActionPerformed(evt);
            }
        });
        fileMenu.add(miSaveAs);

        miClose.setText(resourceMap.getString("miClose.text")); // NOI18N
        miClose.setName("miClose"); // NOI18N
        miClose.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                miCloseActionPerformed(evt);
            }
        });
        fileMenu.add(miClose);

        jSeparator1.setName("jSeparator1"); // NOI18N
        fileMenu.add(jSeparator1);

        javax.swing.ActionMap actionMap = org.jdesktop.application.Application.getInstance(emeraldrandomizer.EmeraldRandomizerAppOld.class).getContext().getActionMap(EmeraldRandomizerView.class, this);
        miExit.setAction(actionMap.get("quit")); // NOI18N
        miExit.setName("miExit"); // NOI18N
        miExit.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                miExitActionPerformed(evt);
            }
        });
        fileMenu.add(miExit);

        menuBar.add(fileMenu);

        mQuickSettings.setText(resourceMap.getString("mQuickSettings.text")); // NOI18N
        mQuickSettings.setName("mQuickSettings"); // NOI18N

        miLow.setText(resourceMap.getString("miLow.text")); // NOI18N
        miLow.setName("miLow"); // NOI18N
        miLow.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                miLowActionPerformed(evt);
            }
        });
        mQuickSettings.add(miLow);

        miMedium.setText(resourceMap.getString("miMedium.text")); // NOI18N
        miMedium.setName("miMedium"); // NOI18N
        miMedium.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                miMediumActionPerformed(evt);
            }
        });
        mQuickSettings.add(miMedium);

        miHigh.setText(resourceMap.getString("miHigh.text")); // NOI18N
        miHigh.setName("miHigh"); // NOI18N
        miHigh.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                miHighActionPerformed(evt);
            }
        });
        mQuickSettings.add(miHigh);

        miArty.setText(resourceMap.getString("miArty.text")); // NOI18N
        miArty.setName("miArty"); // NOI18N
        miArty.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                miArtyActionPerformed(evt);
            }
        });
        mQuickSettings.add(miArty);

        miRandom.setText(resourceMap.getString("miRandom.text")); // NOI18N
        miRandom.setName("miRandom"); // NOI18N
        miRandom.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                miRandomActionPerformed(evt);
            }
        });
        mQuickSettings.add(miRandom);

        jSeparator4.setName("jSeparator4"); // NOI18N
        mQuickSettings.add(jSeparator4);

        miClear.setText(resourceMap.getString("miClear.text")); // NOI18N
        miClear.setName("miClear"); // NOI18N
        miClear.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                miClearActionPerformed(evt);
            }
        });
        mQuickSettings.add(miClear);

        menuBar.add(mQuickSettings);

        helpMenu.setText(resourceMap.getString("helpMenu.text")); // NOI18N
        helpMenu.setName("helpMenu"); // NOI18N

        aboutMenuItem.setAction(actionMap.get("showAboutBox")); // NOI18N
        aboutMenuItem.setName("aboutMenuItem"); // NOI18N
        helpMenu.add(aboutMenuItem);

        menuBar.add(helpMenu);

        setComponent(mainPanel);
        setMenuBar(menuBar);
    }// </editor-fold>//GEN-END:initComponents

    private void miOpenActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_miOpenActionPerformed
        openROM();
    }//GEN-LAST:event_miOpenActionPerformed

    private void miSaveActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_miSaveActionPerformed
        saveFile(romPath);
    }//GEN-LAST:event_miSaveActionPerformed

    private void miSaveAsActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_miSaveAsActionPerformed
        saveFileAs();
    }//GEN-LAST:event_miSaveAsActionPerformed

    private void miCloseActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_miCloseActionPerformed
        if (!saved){
            int ans = JOptionPane.showConfirmDialog(null, "There are still unsaved changes. Save now?");
            if (ans == JOptionPane.CANCEL_OPTION) return;
            if (ans == JOptionPane.YES_OPTION){
                saveFile(romPath);
            } else{
                 leaveFile(romPath);
             }
        }
        setActive(false);
        unsaved(false);
        taWild.setText("Load a ROM to start!\n(Make sure you make a backup first!)");
    }//GEN-LAST:event_miCloseActionPerformed

    private void miExitActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_miExitActionPerformed
        miCloseActionPerformed(null);
    }//GEN-LAST:event_miExitActionPerformed

    private void cbStarter1ItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbStarter1ItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbStarter1ItemStateChanged

    private void cbStarter2ItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbStarter2ItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbStarter2ItemStateChanged

    private void cbStarter3ItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbStarter3ItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbStarter3ItemStateChanged

    private void cbItemItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbItemItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbItemItemStateChanged

    private void rbUnchangedItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_rbUnchangedItemStateChanged
        unsaved(true);
        if (rbUnchanged.isSelected()){
            setWildChoices(false);
        } else {
            setWildChoices(true);
        }
    }//GEN-LAST:event_rbUnchangedItemStateChanged

    private void rbSubsItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_rbSubsItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_rbSubsItemStateChanged

    private void rbGlobalItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_rbGlobalItemStateChanged
        unsaved(true);
        /*if (rbGlobal.isSelected()){
            cbEnsureAll.setSelected(false);
            cbEnsureAll.setEnabled(false);
        } else{
            cbEnsureAll.setEnabled(true);
        }*/
    }//GEN-LAST:event_rbGlobalItemStateChanged

    private void rbRandomItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_rbRandomItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_rbRandomItemStateChanged

    private void rbTrainerUnchangedItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_rbTrainerUnchangedItemStateChanged
        unsaved(true);
        if (rbTrainerUnchanged.isSelected()){
            setTrainerChoices(false);
        } else {
            setTrainerChoices(true);
        }
    }//GEN-LAST:event_rbTrainerUnchangedItemStateChanged

    private void rbTrainerGlobalItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_rbTrainerGlobalItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_rbTrainerGlobalItemStateChanged

    private void rbTrainerRandomItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_rbTrainerRandomItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_rbTrainerRandomItemStateChanged

    private void cbMovesetsItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbMovesetsItemStateChanged
        unsaved(true);
}//GEN-LAST:event_cbMovesetsItemStateChanged

    private void cbRandTMLearnItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbRandTMLearnItemStateChanged
        unsaved(true);
}//GEN-LAST:event_cbRandTMLearnItemStateChanged

    private void cbTMsItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbTMsItemStateChanged
        unsaved(true);
}//GEN-LAST:event_cbTMsItemStateChanged

    private void cbRandStatsItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbRandStatsItemStateChanged
        unsaved(true);
}//GEN-LAST:event_cbRandStatsItemStateChanged

    private void cbRandTypesItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbRandTypesItemStateChanged
        unsaved(true);
}//GEN-LAST:event_cbRandTypesItemStateChanged

    private void cbRandAbilitiesItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbRandAbilitiesItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbRandAbilitiesItemStateChanged

    private void bOpenActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_bOpenActionPerformed
        openROM();
    }//GEN-LAST:event_bOpenActionPerformed

    private void cbEnsureBasicItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbEnsureBasicItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbEnsureBasicItemStateChanged

    private void cbUniqueItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbUniqueItemStateChanged
        unsaved(true);
        if (cbUnique.isSelected()){
            cbUniqueLegends.setEnabled(true);
        } else{
            cbUniqueLegends.setSelected(false);
            cbUniqueLegends.setEnabled(false);
        }
    }//GEN-LAST:event_cbUniqueItemStateChanged

    private void cbUniqueLegendsItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbUniqueLegendsItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbUniqueLegendsItemStateChanged

    private void cbTrainerNamesItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbTrainerNamesItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbTrainerNamesItemStateChanged

    private void cbRivalsItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbRivalsItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbRivalsItemStateChanged

    private void cbFixEvosItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbFixEvosItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbFixEvosItemStateChanged

    private void rbPalNoChangeItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_rbPalNoChangeItemStateChanged
        unsaved(true);
        if (rbPalNoChange.isSelected()){
            setPaletteChoices(false);
        } else {
            setPaletteChoices(true);
        }
    }//GEN-LAST:event_rbPalNoChangeItemStateChanged

    private void rbPalTypeItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_rbPalTypeItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_rbPalTypeItemStateChanged

    private void rbPalRandomItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_rbPalRandomItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_rbPalRandomItemStateChanged

    private void cbPalPrimaryItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbPalPrimaryItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbPalPrimaryItemStateChanged

    private void cbKeepColorsItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbKeepColorsItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbKeepColorsItemStateChanged

    private void bPrintActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_bPrintActionPerformed
        printStatDocs();
    }//GEN-LAST:event_bPrintActionPerformed

    private void bSaveActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_bSaveActionPerformed
        saveFileAs();
    }//GEN-LAST:event_bSaveActionPerformed

    private void cbShinyNormItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbShinyNormItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbShinyNormItemStateChanged

    private void cbRandHeldItemsItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbRandHeldItemsItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbRandHeldItemsItemStateChanged

    private void cbRandPickupItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbRandPickupItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbRandPickupItemStateChanged

    private void cbEnsureAttacksItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbEnsureAttacksItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbEnsureAttacksItemStateChanged

    private void cbRandTrainerHeldItemsItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbRandTrainerHeldItemsItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbRandTrainerHeldItemsItemStateChanged

    private void cbRandTrainerUseItemsItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbRandTrainerUseItemsItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbRandTrainerUseItemsItemStateChanged

    private void bRandomStartersActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_bRandomStartersActionPerformed
        Random rand = new Random();
        cbStarter1.setSelectedIndex(0);
        cbStarter2.setSelectedIndex(0);
        cbStarter3.setSelectedIndex(0);
        cbItem.setSelectedIndex(rand.nextInt(cbItem.getItemCount()));
        unsaved(true);
    }//GEN-LAST:event_bRandomStartersActionPerformed

    private void cbEnsureAllItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbEnsureAllItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbEnsureAllItemStateChanged

    private void cbRandTradesItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbRandTradesItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbRandTradesItemStateChanged

    private void cbRandFieldItemsItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbRandFieldItemsItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbRandFieldItemsItemStateChanged

    private void cbMultTMsItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbMultTMsItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbMultTMsItemStateChanged

    private void cbMetronomeItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbMetronomeItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbMetronomeItemStateChanged

    private void cbNoTrainerLegendsItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbNoTrainerLegendsItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbNoTrainerLegendsItemStateChanged

    private void cbHeartScaleItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbHeartScaleItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbHeartScaleItemStateChanged

    private void cbBattleFrontierItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbBattleFrontierItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbBattleFrontierItemStateChanged

    private void cbNoLegendsWildItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbNoLegendsWildItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbNoLegendsWildItemStateChanged

    private void cbKeepFormItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbKeepFormItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbKeepFormItemStateChanged

    private void miArtyActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_miArtyActionPerformed
        setSelected(
                new boolean[]{true,true,true,true,true,true,true,true,true,true,true,true,false,true,true,true,
                              true,true,true,true,true,true,false,false,true,true,true,true,true,true,true,true,true},
                new int[]{2,2,2}
        );
        unsaved(true);
    }//GEN-LAST:event_miArtyActionPerformed

    private void miHighActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_miHighActionPerformed
        setSelected(
                new boolean[]{false,false,true,false,true,false,false,true,true,true,true,false,false,true,true,true,
                              true,true,true,true,true,true,true,false,false,true,true,true,true,true,true,true,true},
                new int[]{3,2,2}
        );
        unsaved(true);
    }//GEN-LAST:event_miHighActionPerformed

    private void miMediumActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_miMediumActionPerformed
        setSelected(
                new boolean[]{true,false,true,true,true,true,false,true,true,true,false,true,true,false,false,false,
                              true,true,true,true,false,true,false,true,true,true,true,true,true,false,true,true,true},
                new int[]{2,1,1}
        );
        unsaved(true);
    }//GEN-LAST:event_miMediumActionPerformed

    private void miLowActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_miLowActionPerformed
        setSelected(
                new boolean[]{true,true,false,false,true,true,false,true,true,false,false,true,true,false,false,false,
                              false,false,true,true,false,true,false,false,false,false,true,false,false,false,true,true,true},
                new int[]{1,1,0}
        );
        unsaved(true);
    }//GEN-LAST:event_miLowActionPerformed

    private void miRandomActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_miRandomActionPerformed
        Random rand = new Random();
        boolean[] randed = new boolean[32];
        int[] radio = new int[]{rand.nextInt(4),rand.nextInt(3),rand.nextInt(3)};

        for (int i=0;i<randed.length;i++){
            randed[i] = rand.nextBoolean();
        }

        if (radio[0] == 0){
            randed[4] = false;
            randed[5] = false;
            randed[6] = false;
        }
        if (radio[1] == 0){
            randed[11] = false;
            randed[12] = false;
            randed[13] = false;
        }
        if (radio[2] == 0){
            randed[23] = false;
            randed[24] = false;
            randed[25] = false;
        }
        if (!randed[2]) randed[3] = false;
        
        setSelected(randed,radio);
        unsaved(true);
    }//GEN-LAST:event_miRandomActionPerformed

    private void miClearActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_miClearActionPerformed
        boolean[] randed = new boolean[32];

        for (int i=0;i<randed.length;i++){
            randed[i] =false;
        }
        setSelected(randed,new int[]{0,0,0});
        cbStarter1.setSelectedIndex(252);
        cbStarter2.setSelectedIndex(255);
        cbStarter3.setSelectedIndex(258);
        cbItem.setSelectedIndex(0);
        unsaved(true);
    }//GEN-LAST:event_miClearActionPerformed

    private void cbNatlDexItemStateChanged(java.awt.event.ItemEvent evt) {//GEN-FIRST:event_cbNatlDexItemStateChanged
        unsaved(true);
    }//GEN-LAST:event_cbNatlDexItemStateChanged

    private void changeROM(){
        changeStarters(); //must come before trainers
        changeTMs();      //must come before trainers
        randomizePokeData(); //must come before trainers
        changeWildPokemon();
        changePkmnColors();
        fixDisobedience();
        changeIntroPokemon();
        changeTradeEvolutions();
        randomizePickup();
        changeTrainerPokemon();
        changeTrainerClasses();
        heartScales();
        metronomeHyperdrive();
        changeItems();
        changeDex();
    }

    private void checkHash(){
        try{
            MessageDigest digest = MessageDigest.getInstance("MD5");
            digest.update(rom, 0, rom.length);
            String hash = new java.math.BigInteger(1,digest.digest()).toString(16);

            if (!hash.equals("7b058a7aea5bfbb352026727ebd87e17")){
                JOptionPane.showMessageDialog(null, "WARNING: The base ROM does not match the target ROM "+
                        "this program was intended for!\n\nMD5: " + hash + "\nExpected: "+
                        "7b058a7aea5bfbb352026727ebd87e17","Invalid base ROM!",JOptionPane.WARNING_MESSAGE);
            }
        } catch (java.security.NoSuchAlgorithmException nsae){
            JOptionPane.showMessageDialog(null, "Error loading Hash type!","Hash Error",JOptionPane.ERROR_MESSAGE);
        }
    }

    private void changeDex(){
        if (cbNatlDex.isSelected()){
            writeText(addy("e40004"),"[3172016732AC1F083229610825F00129E40825F30116CD40010003]");
            writeText(addy("1fa301"),"[0400e4]");
        }
    }

    private void fixRoamablePkmnText(int[] choices, int[] johto){
        Random rand = new Random();
        String[] pkmn = ak.getPokemonListGameOrder();

        //Latios/Latias
        writeText(addy("1f836e"),"What did the TV host say#that POK@MON looked like");
        writeText(addy("1f82d3"),"looking");
        String[] adjThree = new String[]{"FAT","DIM","SAD"};
        String[] adjFour = new String[]{"ZANY","DULL","CUTE"};
        writeText(addy("5ee14b"),adjThree[rand.nextInt(adjThree.length)]);
        writeText(addy("5ee14f"),adjFour[rand.nextInt(adjFour.length)]);

        //Castform - 23
        StringBuilder sb = replaceInText("[1]",pkmn[choices[23]-1],new StringBuilder("got a [1]!*"));
        writeText(addy("2706eb"),sb.toString());
        String[] castform = new String[]{
                "fills out complex#tax forms for free.$There're plenty of them in the#INSTITUTE--go ahead and take it.*",
                "was duplicated with some#kind of glitch.$There're plenty of them in the#INSTITUTE--go ahead and take it.*",
                "has some sort of#fascination with hats.$There're plenty of them in the#INSTITUTE--go ahead and take it.*"};
        writeText(addy("27070b"),castform[rand.nextInt(castform.length)]);

        //Beldum - 4
        sb = replaceInText("[1]",pkmn[choices[4]-1],new StringBuilder("[1].$Take it?*"));
        writeText(addy("222bd1"),sb.toString());
        sb = replaceInText("[1]",pkmn[choices[4]-1],new StringBuilder("got a [1].*"));
        writeText(addy("222bf0"),sb.toString());
        sb = replaceInText("[1]",pkmn[choices[4]-1],new StringBuilder("[1], my favorite#POK@MON.$Take care of it.$May our "+
                "paths cross someday.$STEVEN STONE*"));
        writeText(addy("222d3b"),sb.toString());

        //Johto trio
        String[][] txtFixes = ak.getJohtoTextFixes();
        sb = replaceInText("[1]",pkmn[johto[1]-1],new StringBuilder(txtFixes[1][rand.nextInt(txtFixes[1].length)]));
        writeText(addy("1fb807"),sb.toString());
        sb = replaceInText("[1]",pkmn[johto[2]-1],new StringBuilder(txtFixes[2][rand.nextInt(txtFixes[2].length)]));
        writeText(addy("1fb87a"),sb.toString());
        sb = replaceInText("[1]",pkmn[johto[0]-1],new StringBuilder(txtFixes[0][rand.nextInt(txtFixes[0].length)]));
        writeText(addy("1fb8f1"),sb.toString());
    }

    private void changeInvisibleItems(){
        int ptr = addy("4824b8");
        Random rand = new Random();
        int[] items = ak.getFindableItems();
        boolean TMs = cbMultTMs.isSelected();
        int temptr;
//        int[] eventCount = new int[4];
//        int[] eventSizes = new int[]{24,8,16,12}; //(person, warp, script, signposts)
//        int[] eventPtrs = new int[4];
        int signPtr,signCount,item;

        for (int i=0;i<518;i++,ptr+=28){
            temptr = makePtr(ptr+4);
            /*for (int p=0;p<4;p++){
                eventCount[p] = byteToInt(rom[temptr+p]);
                eventPtrs[p] = makePtr(temptr+4+4*p);
            }*/
            signCount = byteToInt(rom[temptr+3]);
            signPtr = makePtr(temptr + 16); //4 for count, 12 for the person, warp, and script pointers

            for (int j=0;j<signCount;j++,signPtr+=12){
                if (byteToInt(rom[signPtr+5]) == 7){ //0x07 => data is a hidden item
                    item = byteToInt(rom[signPtr+8]) + byteToInt(rom[signPtr+9])*256;
                    if (indexOf(items,item) != -1){
                        item = items[rand.nextInt(items.length)];
                        rom[signPtr+8] = intToByte(item % 256);
                        rom[signPtr+9] = intToByte(item / 256);
                        //if (TMs && item >= 289){
                        //    rom[signPtr+11] = intToByte(99);
                        //}
                    }
                }
            }
        }

    }

    private int makePtr(int ptr){
        return byteToInt(rom[ptr]) + byteToInt(rom[ptr+1])*256 + byteToInt(rom[ptr+2])*65536;
    }

    private void changeTMQuantities(){
        int ptr = addy("290cd8");
        int oldItem,place;
        ArrayList<Integer> replacers = new ArrayList<Integer>();

        //get the pointers for TM items
        for (int i=0;i<165;i++,ptr+=13){
            oldItem = byteToInt(rom[ptr+3]) + byteToInt(rom[ptr+4])*256;
            if (oldItem >= 289 && oldItem <= 338){
                replacers.add(ptr);
            }
        }

        //adjust quantity
        while (replacers.size() > 0){
            place = replacers.remove(0);
            rom[place+8] = intToByte(95);
        }
    }

    private void changeVisibleItems(){
        int ptr = addy("290cd8");
                //0275 - Eon Ticket
        int oldItem,place;
        boolean TMs = cbMultTMs.isSelected();
        Random rand = new Random();
        ArrayList<Integer> replacers = new ArrayList<Integer>();
        ArrayList<Integer> items = ak.getArrayListInt(ak.getFindableItems());
        ArrayList<Integer> mail = ak.getArrayListInt(new int[]{121,122,123,124,125,126,127,128,129,130,131,132});
        items.addAll(mail);

        for (int i=0;i<165;i++,ptr+=13){
            oldItem = byteToInt(rom[ptr+3]) + byteToInt(rom[ptr+4])*256;
            if (items.contains(oldItem)){
                replacers.add(ptr);
            }
        }

        items.add(275); //Eon Ticket
        items.removeAll(mail); //mail
        //ensure one of each item
        while (items.size() > 0 && replacers.size() > 0){
            place = replacers.remove(rand.nextInt(replacers.size()));
            oldItem = items.remove(rand.nextInt(items.size()));
            if (TMs && oldItem >= 289){ //289 = TM01
                rom[place+8] = intToByte(95);
            }
            rom[place+3] = intToByte(oldItem % 256);
            rom[place+4] = intToByte(oldItem / 256);
        }
        //randomize the rest
        int[] itemsAr = ak.getFindableItems();
        while (replacers.size() > 0){
            place = replacers.remove(rand.nextInt(replacers.size()));
            oldItem = itemsAr[rand.nextInt(itemsAr.length)];
            if (TMs && oldItem >= 289){ //289 = TM01
                rom[place+8] = intToByte(95);
            }
            rom[place+3] = intToByte(oldItem % 256);
            rom[place+4] = intToByte(oldItem / 256);
        }
    }

    private void changeItems(){
        if (!cbRandFieldItems.isSelected()){
            if (cbMultTMs.isSelected()){
                changeTMQuantities();
            }
            return;
        }
        changeVisibleItems();
        changeInvisibleItems(); //shoal stay
    }

    private void metronomeHyperdrive(){
        if (!cbMetronome.isSelected()) return;
        rom[addy("31ce24")] = intToByte(40);   //0x28 => 40 PP

        //ensure all pokemon have Metronome at L:1
        int atkPtr = addy("3230dc");
        int lvl;

        for (int i=0;i<411;i++){
            if (i>=251 && i<276) {//skip 25 empty slots
                atkPtr += 4;
            } else{
                //note that ALL pokemon have at least 1 move, so this should be executed alright
                lvl = byteToInt(rom[atkPtr+1])>>1;
                writeAttack(atkPtr,lvl,118); //118 = metronome
                atkPtr += 2;

                //move to next pokemon
                while (byteToInt(rom[atkPtr])!=255 || byteToInt(rom[atkPtr+1])!=255){
                    atkPtr += 2;
                }
                atkPtr += 2;    //pass final FFFF
            }
        }
    }

    private void heartScales(){
        if (cbHeartScale.isSelected()){
            rom[addy("1ffce2")] = intToByte(111);   //0x6f
            rom[addy("1ffce3")] = intToByte(0);     //0x00

            int ptr = addy("1ffce4");
            int stone = 93;
            for (int i=0;i<6;i++,ptr+=2){
                rom[ptr] = intToByte(stone+i);
                rom[ptr+1] = intToByte(0);
            }
        }
    }

    private void changeTrainerClasses(){
        if (!cbTrainerNames.isSelected()) return;
        String[][] classes = ak.getTrainerTitles();
        int ptr = addy("30fcd4");
        Random rand = new Random();
        int[] data;
        int p;

        for (int i=0;i<classes.length;i++,ptr+=13){
            data = encodeText(classes[i][rand.nextInt(5)]);
            for (p=0;p<data.length;p++){
                rom[ptr+p] = intToByte(data[p]);
            }
            rom[ptr+p] = intToByte(255);    //terminal 0xFF

            for (p++;ptr < 12;ptr++){
                rom[ptr+p] = intToByte(0);  //end with all 0x00
            }
        }
    }

    private void changeIntroPokemon(){
        Random rand = new Random();
        int[] pkmn = ak.getPokemonListGame();
        int chox = pkmn[rand.nextInt(pkmn.length)];
        
        int ptr = addy("30b0c");
        rom[ptr] = intToByte(chox%256);
        rom[ptr+1] = intToByte(chox/256);
        ptr = addy("31924");
        rom[ptr] = intToByte(chox%256);
        rom[ptr+1] = intToByte(chox/256);
    }

    private void changeTrainerPokemon(){        
        int ptr = addy("310058");
        int pkmnPtr;
        Random rand = new Random();
        boolean[] choices = new boolean[]{cbRandTrainerHeldItems.isSelected(),cbRandTrainerUseItems.isSelected(),
                                        cbNoTrainerLegends.isSelected(), cbBattleFrontier.isSelected()};
        int[] itemList = ak.getUsableItems();
        int[] battleItems = ak.getBattleUseItems();
        ArrayList<Integer> usedAt = new ArrayList<Integer>();
        ArrayList<Integer> fullEvo = ak.getArrayListInt(ak.getTrainersFullEvo());
        int dataType = byteToInt(rom[ptr]);
        int numPkmn = 1;
        int item = 0;
        int pkmn = 0;
        int move = 0;

        if (rbTrainerUnchanged.isSelected()){
            if (!choices[0] && !choices[1]){ //no item changes; we're done here
                return;
            }
            for (;dataType <= 3;ptr += 40){
                pkmnPtr = byteToInt(rom[ptr+38]) * 65536 + byteToInt(rom[ptr+37]) * 256 + byteToInt(rom[ptr+36]);
                numPkmn = byteToInt(rom[ptr+32]);

                //used items - only if there's an item
                if (choices[1]){
                    for (int p=0;p<4;p++){
                        if (byteToInt(rom[ptr+17+2*p]) * 256 + byteToInt(rom[ptr+16+2*p]) != 0){
                            item = battleItems[rand.nextInt(battleItems.length)];
                            rom[ptr+17+2*p] = intToByte(item/256);
                            rom[ptr+16+2*p] = intToByte(item%256);
                        }
                    }
                }
                
                switch (dataType){
                    case 1:     //special moves ------------------------------------
                        pkmnPtr += 16*numPkmn;
                        break;
                    case 2:     //item -------------------------------------------------------
                        for (int i=0;i<numPkmn;i++,pkmnPtr += 8){
                            //item
                            if (choices[0]){
                                item = itemList[rand.nextInt(itemList.length)];
                                rom[pkmnPtr+7] = intToByte(item / 256);
                                rom[pkmnPtr+6] = intToByte(item % 256);
                            }
                        }
                        break;
                    case 3:     //item, special moves ----------------------------------------
                        for (int i=0;i<numPkmn;i++,pkmnPtr += 16){
                            //item
                            if (choices[0]){
                                item = itemList[rand.nextInt(itemList.length)];
                                rom[pkmnPtr+7] = intToByte(item / 256);
                                rom[pkmnPtr+6] = intToByte(item % 256);
                            }
                        }
                        break;
                    default:    //pokemon only ------------------------------------------------
                        pkmnPtr += 8*numPkmn;
                }
                dataType = byteToInt(rom[ptr+40]);
            }
            return; //don't need to do rival pokemon swapping
        } else if (rbTrainerGlobal.isSelected()){ //====================================================================
            HashMap<Integer,Integer> pkmnSwap = new HashMap<Integer,Integer>();
            ArrayList<Integer> pkmnListChoices = generatePokemonList(choices[2]);
            ArrayList<ArrayList<Integer>> possMoves = getPossibleTMTutorAttacks();
            ArrayList<Integer> currMoves;
            int oldPkmn = 0;
            int[][] family = ak.getFamilyTree();
            boolean isFull = false;

            for (int t=0;dataType <= 3;ptr += 40,t++){
                pkmnPtr = byteToInt(rom[ptr+38]) * 65536 + byteToInt(rom[ptr+37]) * 256 + byteToInt(rom[ptr+36]);
                numPkmn = byteToInt(rom[ptr+32]);
                if (!fullEvo.isEmpty() && t == fullEvo.get(0)){
                    isFull = true;
                    fullEvo.remove(0);
                } else{
                    isFull = false;
                }

                //used items - only if there's an item
                if (choices[1]){
                    for (int p=0;p<4;p++){
                        if (byteToInt(rom[ptr+17+2*p]) * 256 + byteToInt(rom[ptr+16+2*p]) != 0){
                            item = battleItems[rand.nextInt(battleItems.length)];
                            rom[ptr+17+2*p] = intToByte(item/256);
                            rom[ptr+16+2*p] = intToByte(item%256);
                        }
                    }
                }
                switch (dataType){
                    case 1:     //special moves ------------------------------------
                        for (int i=0;i<numPkmn;i++,pkmnPtr += 16){
                            //pokemon
                            oldPkmn = byteToInt(rom[pkmnPtr+5]) * 256 + byteToInt(rom[pkmnPtr+4]);
                            if (pkmnSwap.keySet().contains(oldPkmn)){
                                pkmn = pkmnSwap.get(oldPkmn);
                            } else{
                                pkmn = pkmnListChoices.remove(rand.nextInt(pkmnListChoices.size()));
                                pkmnSwap.put(oldPkmn, pkmn);
                                if (pkmnListChoices.size() == 0){
                                    pkmnListChoices = generatePokemonList(choices[2]);
                                }
                            }
                            if (isFull){ //make it a full evolution
                                pkmn = family[pkmn-1][family[pkmn-1].length-1];
                            }
                            rom[pkmnPtr+5] = intToByte(pkmn / 256);
                            rom[pkmnPtr+4] = intToByte(pkmn % 256);

                            //moves
                            currMoves = possMoves.get(pkmn-1);
                            for (int p=0;p<4;p++){
                                move = byteToInt(rom[pkmnPtr+9+p*2]) * 256 + byteToInt(rom[pkmnPtr+8+p*2]);
                                if (move > 0){
                                    if (currMoves.size() <= 0){
                                        rom[pkmnPtr+9+p*2] = intToByte(0);
                                        rom[pkmnPtr+8+p*2] = intToByte(0);
                                        continue;   //pokemon doesn't learn enough moves
                                    }
                                    move = currMoves.remove(rand.nextInt(currMoves.size()));
                                    rom[pkmnPtr+9+p*2] = intToByte(move / 256);
                                    rom[pkmnPtr+8+p*2] = intToByte(move % 256);
                                    usedAt.add(move);
                                }
                            }
                            //replace attack list
                            while (usedAt.size() > 0){
                                currMoves.add(usedAt.remove(0));
                            }
                        }
                        break;
                    case 2:     //item -------------------------------------------------------
                        for (int i=0;i<numPkmn;i++,pkmnPtr += 8){
                            //pokemon
                            oldPkmn = byteToInt(rom[pkmnPtr+5]) * 256 + byteToInt(rom[pkmnPtr+4]);
                            if (pkmnSwap.keySet().contains(oldPkmn)){
                                pkmn = pkmnSwap.get(oldPkmn);
                            } else{
                                pkmn = pkmnListChoices.remove(rand.nextInt(pkmnListChoices.size()));
                                pkmnSwap.put(oldPkmn, pkmn);
                                if (pkmnListChoices.size() == 0){
                                    pkmnListChoices = generatePokemonList(choices[2]);
                                }
                            }
                            if (isFull){ //make it a full evolution
                                pkmn = family[pkmn-1][family[pkmn-1].length-1];
                            }
                            rom[pkmnPtr+5] = intToByte(pkmn / 256);
                            rom[pkmnPtr+4] = intToByte(pkmn % 256);
                            //item
                            if (choices[0]){
                                item = itemList[rand.nextInt(itemList.length)];
                                rom[pkmnPtr+7] = intToByte(item / 256);
                                rom[pkmnPtr+6] = intToByte(item % 256);
                            }
                        }
                        break;
                    case 3:     //item, special moves ----------------------------------------
                        for (int i=0;i<numPkmn;i++,pkmnPtr += 16){
                            //pokemon
                            oldPkmn = byteToInt(rom[pkmnPtr+5]) * 256 + byteToInt(rom[pkmnPtr+4]);
                            if (pkmnSwap.keySet().contains(oldPkmn)){
                                pkmn = pkmnSwap.get(oldPkmn);
                            } else{
                                pkmn = pkmnListChoices.remove(rand.nextInt(pkmnListChoices.size()));
                                pkmnSwap.put(oldPkmn, pkmn);
                                if (pkmnListChoices.size() == 0){
                                    pkmnListChoices = generatePokemonList(choices[2]);
                                }
                            }
                            if (isFull){ //make it a full evolution
                                pkmn = family[pkmn-1][family[pkmn-1].length-1];
                            }
                            rom[pkmnPtr+5] = intToByte(pkmn / 256);
                            rom[pkmnPtr+4] = intToByte(pkmn % 256);
                            //moves
                            currMoves = possMoves.get(pkmn-1);
                            for (int p=0;p<4;p++){
                                move = byteToInt(rom[pkmnPtr+9+p*2]) * 256 + byteToInt(rom[pkmnPtr+8+p*2]);
                                if (move > 0){
                                    if (currMoves.size() <= 0){
                                        rom[pkmnPtr+9+p*2] = intToByte(0);
                                        rom[pkmnPtr+8+p*2] = intToByte(0);
                                        continue;   //pokemon doesn't learn enough moves
                                    }
                                    move = currMoves.remove(rand.nextInt(currMoves.size()));
                                    rom[pkmnPtr+9+p*2] = intToByte(move / 256);
                                    rom[pkmnPtr+8+p*2] = intToByte(move % 256);
                                    usedAt.add(move);
                                }
                            }
                            //replace attack list
                            while (usedAt.size() > 0){
                                currMoves.add(usedAt.remove(0));
                            }
                        }
                        break;
                    default:    //pokemon only ------------------------------------------------
                        for (int i=0;i<numPkmn;i++,pkmnPtr += 8){
                            //pokemon
                            oldPkmn = byteToInt(rom[pkmnPtr+5]) * 256 + byteToInt(rom[pkmnPtr+4]);
                            if (pkmnSwap.keySet().contains(oldPkmn)){
                                pkmn = pkmnSwap.get(oldPkmn);
                            } else{
                                pkmn = pkmnListChoices.remove(rand.nextInt(pkmnListChoices.size()));
                                pkmnSwap.put(oldPkmn, pkmn);
                                if (pkmnListChoices.size() == 0){
                                    pkmnListChoices = generatePokemonList(choices[2]);
                                }
                            }
                            if (isFull){ //make it a full evolution
                                pkmn = family[pkmn-1][family[pkmn-1].length-1];
                            }
                            rom[pkmnPtr+5] = intToByte(pkmn / 256);
                            rom[pkmnPtr+4] = intToByte(pkmn % 256);
                        }
                }
                dataType = byteToInt(rom[ptr+40]);
            }

            //Battle Frontier
            if (choices[3]){
                ArrayList<ArrayList<Integer>> attacks = getPossibleAttacks();
                int bfPtr = addy("5d97bc"); //battle frontier pokemon
                int fbPtr = addy("61156c"); //frontier brain pokemon
                int[] evs = ak.getFrontierEVs();

                //battle frontier
                for (int r=0;r<882;r++,bfPtr+=16){ //882 pokemon total
                    //pokemon
                    oldPkmn = byteToInt(rom[bfPtr+1]) * 256 + byteToInt(rom[bfPtr]);
                    if (pkmnSwap.keySet().contains(oldPkmn)){
                        pkmn = pkmnSwap.get(oldPkmn);
                    } else{
                        pkmn = pkmnListChoices.remove(rand.nextInt(pkmnListChoices.size()));
                        pkmnSwap.put(oldPkmn, pkmn);
                        if (pkmnListChoices.size() == 0){
                            pkmnListChoices = generatePokemonList(choices[2]);
                        }
                    }
                    rom[bfPtr] = intToByte(pkmn % 256);
                    rom[bfPtr+1] = intToByte(pkmn / 256);

                    //attacks
                    ArrayList<Integer> possAttacks = attacks.get(pkmn-1);
                    int at=0;
                    for (int m=0;m<4;m++){
                        if (possAttacks.size() <= 0){
                            rom[bfPtr+2+m*2] = intToByte(0);
                            rom[bfPtr+3+m*2] = intToByte(0);
                            continue;   //pokemon doesn't learn enough moves
                        }
                        at = possAttacks.remove(rand.nextInt(possAttacks.size()));
                        usedAt.add(at);
                        rom[bfPtr+2+m*2] = intToByte(at%256);
                        rom[bfPtr+3+m*2] = intToByte(at/256);
                    }
                    while (usedAt.size() > 0){
                        possAttacks.add(usedAt.remove(0));
                    }

                    //stats,item
                    rom[bfPtr+12] = intToByte(rand.nextInt(25));
                    rom[bfPtr+11] = intToByte(evs[rand.nextInt(evs.length)]);
                    if (choices[0]){
                        rom[bfPtr+10] = intToByte(rand.nextInt(62)+1);
                    }
                }

                //frontier brains
                int[] items = ak.getBattleHeldItems();
                int[] fam;
                for (int r=0;r<42;r++,fbPtr+=20){ //42 pokemon total
                    //pokemon
                    oldPkmn = byteToInt(rom[fbPtr+1]) * 256 + byteToInt(rom[fbPtr]);
                    if (pkmnSwap.keySet().contains(oldPkmn)){
                        pkmn = pkmnSwap.get(oldPkmn);
                    } else{
                        fam = family[pkmn-1];
                        pkmn = pkmnListChoices.remove(rand.nextInt(pkmnListChoices.size()));
                        if (fam.length > 1 && indexOf(fam,pkmn) == 0){
                            pkmn = fam[rand.nextInt(fam.length-1)+1];
                        }
                        pkmnSwap.put(oldPkmn, pkmn);
                        if (pkmnListChoices.size() == 0){
                            pkmnListChoices = generatePokemonList(choices[2]);
                        }
                    }
                    rom[fbPtr] = intToByte(pkmn % 256);
                    rom[fbPtr+1] = intToByte(pkmn / 256);

                    //attacks
                    ArrayList<Integer> possAttacks = attacks.get(pkmn-1);
                    int at=0;
                    for (int m=0;m<4;m++){
                        if (possAttacks.size() <= 0){
                            rom[bfPtr+2+m*2] = intToByte(0);
                            rom[bfPtr+3+m*2] = intToByte(0);
                            continue;   //pokemon doesn't learn enough moves
                        }
                        at = possAttacks.remove(rand.nextInt(possAttacks.size()));
                        usedAt.add(at);
                        rom[fbPtr+12+m*2] = intToByte(at%256);
                        rom[fbPtr+13+m*2] = intToByte(at/256);
                    }
                    while (usedAt.size() > 0){
                        possAttacks.add(usedAt.remove(0));
                    }

                    //items
                    if (choices[0]){
                        item = items[rand.nextInt(items.length)];
                        rom[fbPtr+2] = intToByte(item % 256);
                        rom[fbPtr+3] = intToByte(item / 256);
                    }
                }
            }
            //Steven's pokemon
            ArrayList<ArrayList<Integer>> attacks = getPossibleAttacks();
            int[] fam;
            ptr = addy("5dd6d0");
            for (int i=0;i<3;i++,ptr+=20){ //3 pokemon
                //pokemon
                oldPkmn = byteToInt(rom[ptr+1]) * 256 + byteToInt(rom[ptr]);
                if (pkmnSwap.keySet().contains(oldPkmn)){
                    pkmn = pkmnSwap.get(oldPkmn);
                } else{
                    fam = family[pkmn-1];
                    pkmn = pkmnListChoices.remove(rand.nextInt(pkmnListChoices.size()));
                    if (fam.length > 1 && indexOf(fam,pkmn) == 0){
                        pkmn = fam[rand.nextInt(fam.length-1)+1];
                    }
                    pkmnSwap.put(oldPkmn, pkmn);
                    if (pkmnListChoices.size() == 0){
                        pkmnListChoices = generatePokemonList(choices[2]);
                    }
                }
                rom[ptr] = intToByte(pkmn % 256);
                rom[ptr+1] = intToByte(pkmn / 256);

                //attacks
                ArrayList<Integer> possAttacks = attacks.get(pkmn-1);
                int at=0;
                for (int m=0;m<4;m++){
                    if (possAttacks.size() <= 0){
                        rom[ptr+12+m*2] = intToByte(0);
                        rom[ptr+13+m*2] = intToByte(0);
                        continue;   //pokemon doesn't learn enough moves
                    }
                    at = possAttacks.remove(rand.nextInt(possAttacks.size()));
                    usedAt.add(at);
                    rom[ptr+12+m*2] = intToByte(at%256);
                    rom[ptr+13+m*2] = intToByte(at/256);
                }
                while (usedAt.size() > 0){
                    possAttacks.add(usedAt.remove(0));
                }
            }
        } else{ //rbTrainerRandom =============================================================================
            ArrayList<Integer> pkmnList = generatePokemonList(choices[2]);
            ArrayList<ArrayList<Integer>> possMoves = getPossibleTMTutorAttacks();
            ArrayList<Integer> currMoves;
            int[][] family = ak.getFamilyTree();
            boolean isFull = false;

            for (int t=0;dataType <= 3;ptr += 40, t++){
                pkmnPtr = byteToInt(rom[ptr+38]) * 65536 + byteToInt(rom[ptr+37]) * 256 + byteToInt(rom[ptr+36]);
                numPkmn = byteToInt(rom[ptr+32]);
                if (!fullEvo.isEmpty() && t == fullEvo.get(0)){
                    isFull = true;
                    fullEvo.remove(0);
                } else{
                    isFull = false;
                }

                //used items - only if there's an item
                if (choices[1]){
                    for (int p=0;p<4;p++){
                        if (byteToInt(rom[ptr+17+2*p]) * 256 + byteToInt(rom[ptr+16+2*p]) != 0){
                            item = battleItems[rand.nextInt(battleItems.length)];
                            rom[ptr+17+2*p] = intToByte(item/256);
                            rom[ptr+16+2*p] = intToByte(item%256);
                        }
                    }
                }
                
                switch (dataType){
                    case 1:     //special moves ------------------------------------
                        for (int i=0;i<numPkmn;i++,pkmnPtr += 16){
                            //pokemon
                            pkmn = pkmnList.get(rand.nextInt(pkmnList.size()));
                            if (isFull){ //make it a full evolution
                                pkmn = family[pkmn-1][family[pkmn-1].length-1];
                            }
                            rom[pkmnPtr+5] = intToByte(pkmn / 256);
                            rom[pkmnPtr+4] = intToByte(pkmn % 256);
                            //moves
                            currMoves = possMoves.get(pkmn-1);
                            for (int p=0;p<4;p++){
                                move = byteToInt(rom[pkmnPtr+9+p*2]) * 256 + byteToInt(rom[pkmnPtr+8+p*2]);
                                if (move > 0){
                                    if (currMoves.size() <= 0){
                                        rom[pkmnPtr+9+p*2] = intToByte(0);
                                        rom[pkmnPtr+8+p*2] = intToByte(0);
                                        continue;   //pokemon doesn't learn enough moves
                                    }
                                    move = currMoves.remove(rand.nextInt(currMoves.size()));
                                    rom[pkmnPtr+9+p*2] = intToByte(move / 256);
                                    rom[pkmnPtr+8+p*2] = intToByte(move % 256);
                                    usedAt.add(move);
                                }
                            }
                            //replace attack list
                            while (usedAt.size() > 0){
                                currMoves.add(usedAt.remove(0));
                            }
                        }
                        break;
                    case 2:     //item -------------------------------------------------------
                        for (int i=0;i<numPkmn;i++,pkmnPtr += 8){
                            //pokemon
                            pkmn = pkmnList.get(rand.nextInt(pkmnList.size()));
                            if (isFull){ //make it a full evolution
                                pkmn = family[pkmn-1][family[pkmn-1].length-1];
                            }
                            rom[pkmnPtr+5] = intToByte(pkmn / 256);
                            rom[pkmnPtr+4] = intToByte(pkmn % 256);
                            //item
                            if (choices[0]){
                                item = itemList[rand.nextInt(itemList.length)];
                                rom[pkmnPtr+7] = intToByte(item / 256);
                                rom[pkmnPtr+6] = intToByte(item % 256);
                            }
                        }
                        break;
                    case 3:     //item, special moves ----------------------------------------
                        for (int i=0;i<numPkmn;i++,pkmnPtr += 16){
                            //pokemon
                            pkmn = pkmnList.get(rand.nextInt(pkmnList.size()));
                            if (isFull){ //make it a full evolution
                                pkmn = family[pkmn-1][family[pkmn-1].length-1];
                            }
                            rom[pkmnPtr+5] = intToByte(pkmn / 256);
                            rom[pkmnPtr+4] = intToByte(pkmn % 256);
                            //item
                            if (choices[0]){
                                item = itemList[rand.nextInt(itemList.length)];
                                rom[pkmnPtr+7] = intToByte(item / 256);
                                rom[pkmnPtr+6] = intToByte(item % 256);
                            }
                            //moves
                            currMoves = possMoves.get(pkmn-1);
                            for (int p=0;p<4;p++){
                                move = byteToInt(rom[pkmnPtr+9+p*2]) * 256 + byteToInt(rom[pkmnPtr+8+p*2]);
                                if (move > 0){
                                    if (currMoves.size() <= 0){
                                        rom[pkmnPtr+9+p*2] = intToByte(0);
                                        rom[pkmnPtr+8+p*2] = intToByte(0);
                                        continue;   //pokemon doesn't learn enough moves
                                    }
                                    move = currMoves.remove(rand.nextInt(currMoves.size()));
                                    rom[pkmnPtr+9+p*2] = intToByte(move / 256);
                                    rom[pkmnPtr+8+p*2] = intToByte(move % 256);
                                    usedAt.add(move);
                                }
                            }
                            //replace attack list
                            while (usedAt.size() > 0){
                                currMoves.add(usedAt.remove(0));
                            }
                        }
                        break;
                    default:    //pokemon only ------------------------------------------------
                        for (int i=0;i<numPkmn;i++,pkmnPtr += 8){
                            //pokemon
                            pkmn = pkmnList.get(rand.nextInt(pkmnList.size()));
                            if (isFull){ //make it a full evolution
                                pkmn = family[pkmn-1][family[pkmn-1].length-1];
                            }
                            rom[pkmnPtr+5] = intToByte(pkmn / 256);
                            rom[pkmnPtr+4] = intToByte(pkmn % 256);
                        }
                }
                dataType = byteToInt(rom[ptr+40]);
            }

            //Battle Frontier
            if (choices[3]){
                ArrayList<ArrayList<Integer>> attacks = getPossibleAttacks();
                int bfPtr = addy("5d97bc"); //battle frontier pokemon
                int fbPtr = addy("61156c"); //frontier brain pokemon
                int[] evs = ak.getFrontierEVs();

                //battle frontier
                for (int r=0;r<882;r++,bfPtr+=16){ //882 pokemon total
                    //pokemon
                    pkmn = pkmnList.get(rand.nextInt(pkmnList.size()));
                    rom[bfPtr] = intToByte(pkmn % 256);
                    rom[bfPtr+1] = intToByte(pkmn / 256);

                    //attacks
                    ArrayList<Integer> possAttacks = attacks.get(pkmn-1);
                    int at=0;
                    for (int m=0;m<4;m++){
                        if (possAttacks.size() <= 0){
                            rom[bfPtr+2+m*2] = intToByte(0);
                            rom[bfPtr+3+m*2] = intToByte(0);
                            continue;   //pokemon doesn't learn enough moves
                        }
                        at = possAttacks.remove(rand.nextInt(possAttacks.size()));
                        usedAt.add(at);
                        rom[bfPtr+2+m*2] = intToByte(at%256);
                        rom[bfPtr+3+m*2] = intToByte(at/256);
                    }
                    while (usedAt.size() > 0){
                        possAttacks.add(usedAt.remove(0));
                    }

                    //stats,item
                    rom[bfPtr+12] = intToByte(rand.nextInt(25));
                    rom[bfPtr+11] = intToByte(evs[rand.nextInt(evs.length)]);
                    if (choices[0]){
                        rom[bfPtr+10] = intToByte(rand.nextInt(62)+1);
                    }
                }

                //frontier brains
                int[] items = ak.getBattleHeldItems();
                int[] fam;
                for (int r=0;r<42;r++,fbPtr+=20){ //42 pokemon total
                    //pokemon
                    pkmn = pkmnList.get(rand.nextInt(pkmnList.size()));
                    fam = family[pkmn-1];
                    if (fam.length > 1 && indexOf(fam,pkmn) == 0){
                        pkmn = fam[rand.nextInt(fam.length-1)+1];
                    }
                    rom[fbPtr] = intToByte(pkmn % 256);
                    rom[fbPtr+1] = intToByte(pkmn / 256);

                    //attacks
                    ArrayList<Integer> possAttacks = attacks.get(pkmn-1);
                    int at=0;
                    for (int m=0;m<4;m++){
                        if (possAttacks.size() <= 0){
                            rom[bfPtr+2+m*2] = intToByte(0);
                            rom[bfPtr+3+m*2] = intToByte(0);
                            continue;   //pokemon doesn't learn enough moves
                        }
                        at = possAttacks.remove(rand.nextInt(possAttacks.size()));
                        usedAt.add(at);
                        rom[fbPtr+12+m*2] = intToByte(at%256);
                        rom[fbPtr+13+m*2] = intToByte(at/256);
                    }
                    while (usedAt.size() > 0){
                        possAttacks.add(usedAt.remove(0));
                    }

                    //items
                    if (choices[0]){
                        item = items[rand.nextInt(items.length)];
                        rom[fbPtr+2] = intToByte(item % 256);
                        rom[fbPtr+3] = intToByte(item / 256);
                    }
                }
            }

            //Steven's
            ptr = addy("5dd6d0");
            ArrayList<ArrayList<Integer>> attacks = getPossibleAttacks();
            int[] fam;
            for (int i=0;i<3;i++,ptr+=20){ //3 pokemon
                //pokemon
                pkmn = pkmnList.get(rand.nextInt(pkmnList.size()));
                fam = family[pkmn-1];
                if (fam.length > 1 && indexOf(fam,pkmn) == 0){
                    pkmn = fam[rand.nextInt(fam.length-1)+1];
                }
                rom[ptr] = intToByte(pkmn % 256);
                rom[ptr+1] = intToByte(pkmn / 256);

                //attacks
                ArrayList<Integer> possAttacks = attacks.get(pkmn-1);
                int at=0;
                for (int m=0;m<4;m++){
                    if (possAttacks.size() <= 0){
                        rom[ptr+12+m*2] = intToByte(0);
                        rom[ptr+13+m*2] = intToByte(0);
                        continue;   //pokemon doesn't learn enough moves
                    }
                    at = possAttacks.remove(rand.nextInt(possAttacks.size()));
                    usedAt.add(at);
                    rom[ptr+12+m*2] = intToByte(at%256);
                    rom[ptr+13+m*2] = intToByte(at/256);
                }
                while (usedAt.size() > 0){
                    possAttacks.add(usedAt.remove(0));
                }
            }
        }

        if (cbRivals.isSelected()){ //make rival pokemon
            //pointers for starter pokemon
            int[] wally = new int[]{addy("30e550"),addy("30db20"),addy("30e598"),addy("30e5e8"),addy("30e638"),addy("30e688")};
            int[] brendan = new int[]{addy("30db30"),addy("30e098"),addy("30db48"),addy("30db60"),addy("30e6b0"),
                                      addy("30db68"),addy("30e088"),addy("30db80"),addy("30db98"),addy("30e6d0"),
                                      addy("30dba0"),addy("30e100"),addy("30dbb8"),addy("30dbd0"),addy("30e6f0")};
            int[] may = new int[]{addy("30dbd8"),addy("30e110"),addy("30dbf0"),addy("30dc08"),addy("30e710"),
                                  addy("30dc10"),addy("30edb0"),addy("30dc28"),addy("30dc40"),addy("30e730"),
                                  addy("30dc48"),addy("30edc0"),addy("30dc60"),addy("30dc78"),addy("30e750")};
            int[] basicPokemon = ak.getBasicPokemonGame();
            int wallyStart = basicPokemon[rand.nextInt(basicPokemon.length)];
            int[] nonMales = ak.getMaleGenderOptions();
            while (indexOf(nonMales,wallyStart) != -1){
                wallyStart = basicPokemon[rand.nextInt(basicPokemon.length)];
            }
            int wallyBattlePtr = addy("B0870");
            int[] evoCount = ak.getEvoCount();
            int[][] familyTree = ak.getFamilyTree();

            //change the Wally battle
            if (wallyStart <= 255){
                writeToROM(wallyBattlePtr,new int[]{wallyStart,33,28,0}); // ##21c00
            } else if(wallyStart > 255+256){
                JOptionPane.showMessageDialog(null, "Error setting Wally's starter battle.\n"
                        + "No change was made.");
            }else{
                writeToROM(wallyBattlePtr,new int[]{wallyStart-255,33,255,49}); //##21FB31
            }

            //Wally battles
            rom[wally[0]] = intToByte(wallyStart % 256);
            rom[wally[0]+1] = intToByte(wallyStart / 256);

            if (evoCount[wallyStart-1] != 0) { //it evolves -- get last family member
                wallyStart = familyTree[wallyStart-1][familyTree[wallyStart-1].length-1];
            }

            for (int p=1;p<wally.length;p++){
                rom[wally[p]] = intToByte(wallyStart % 256);
                rom[wally[p]+1] = intToByte(wallyStart / 256);
            }

            //change NPC text for Wally
            String[] pkmnNames = ak.getPokemonListGameOrder();
            StringBuilder sb = new StringBuilder("I have a new purpose in life.#Together with my [1], I'm going^to "+
                    "challenge POK@MON GYMS and become^a great TRAINER.$Please watch me, [FD01].#I'm going to be "+
                    "stronger than you.$When I do, I'm going to challenge you#to another battle.*");
            sb = replaceInText("[1]",pkmnNames[wallyStart-1],sb);
            writeText(addy("2029cf"),sb.toString());

            sb = new StringBuilder("fight with [1],#we can beat anyone!*");
            sb = replaceInText("[1]",pkmnNames[wallyStart-1],sb);
            writeText(addy("1df983"),sb.toString());

            //get starters
            int startPtr = addy("5b1df8");
            int[] starters = {byteToInt(rom[startPtr]) + byteToInt(rom[startPtr+1])*256,
                            byteToInt(rom[startPtr+2]) + byteToInt(rom[startPtr+3])*256,
                            byteToInt(rom[startPtr+4]) + byteToInt(rom[startPtr+5])*256,};

            for (int p=0;p<5;p++){ //first 3 battles are unevolved
                rom[brendan[p]] = intToByte(starters[0] % 256);
                rom[brendan[p]+1] = intToByte(starters[0] / 256);
                rom[brendan[p+5]] = intToByte(starters[1] % 256);
                rom[brendan[p+5]+1] = intToByte(starters[1] / 256);
                rom[brendan[p+10]] = intToByte(starters[2] % 256);
                rom[brendan[p+10]+1] = intToByte(starters[2] / 256);
                rom[may[p]] = intToByte(starters[0] % 256);
                rom[may[p]+1] = intToByte(starters[0] / 256);
                rom[may[p+5]] = intToByte(starters[1] % 256);
                rom[may[p+5]+1] = intToByte(starters[1] / 256);
                rom[may[p+10]] = intToByte(starters[2] % 256);
                rom[may[p+10]+1] = intToByte(starters[2] / 256);

                if (p==1){
                    //evolve pokemon, if need be
                    for (int q=0;q<starters.length;q++){
                        if (evoCount[starters[q]] != 0) { //it evolves -- get last family member
                            int[] fam = familyTree[starters[q]-1];
                            int index = indexOf(fam,starters[q]);
                            starters[q] = fam[index];
                        }
                    }
                }
            }
        } else{
            //change the Wally battle
            int wallyBattlePtr = addy("B0870");
            int[] pkmnList = ak.getPokemonListGame();
            int wallyStart = pkmnList[rand.nextInt(pkmnList.length)];
            if (wallyStart <= 255){
                writeToROM(wallyBattlePtr,new int[]{wallyStart,33,28,0}); // ##21c000
            } else if(wallyStart > 255+256){
                JOptionPane.showMessageDialog(null, "Error setting Wally's starter battle.\n"
                        + "No change was made.");
            }else{
                writeToROM(wallyBattlePtr,new int[]{wallyStart-255,33,255,49}); //##21FF31
            }

            //change NPC text for Wally
            String[] pkmnNames = ak.getPokemonListGameOrder();
            StringBuilder sb = new StringBuilder("I have a new purpose in life.#Together with my POK@MON, I'm going^to "+
                    "challenge POK@MON GYMS and become^a great TRAINER.$Please watch me, [FD01].#I'm going to be "+
                    "stronger than you.$When I do, I'm going to challenge you#to another battle.*");
            writeText(addy("2029cf"),sb.toString());

            sb = new StringBuilder("fight with POK@MON,#we can beat anyone!*");
            writeText(addy("1df983"),sb.toString());

        }
                    
        //randomize Wally's borrowed pokemon
        int[] pkmnList = ak.getPokemonListGame();
        int poke = pkmnList[rand.nextInt(pkmnList.length)];
        int lentPtr = addy("139472");
        
        StringBuilder sb = new StringBuilder("a POK@MON.$WALLY received a "+ak.getPokemonListGameOrder()[poke-1] + "!");
        while (sb.length() < 39){
            sb.append(' ');
        }

        if (poke <= 255){
            writeToROM(lentPtr,new int[]{poke,33,0,0}); // ##210000
            writeText(addy("205a61"),sb.toString());
        } else if(poke > 255+256){
            JOptionPane.showMessageDialog(null, "Error setting Wally's borrowed pokemon.\n"
                    + "No change was made.");
        } else{
            writeToROM(lentPtr,new int[]{poke-255,33,255,49}); //##21FF31
            writeText(addy("205a61"),sb.toString());
        }
    }

    private ArrayList<ArrayList<Integer>> getPossibleAttacks(){
        ArrayList<Integer> currPkmn;
        ArrayList<ArrayList<Integer>> out = new ArrayList<ArrayList<Integer>>();
        int[] TMHMlist = getTMHMlist();
        int[] tutorList = getTutorList();
        int TMptr = addy("31e8a0");
        int tutorPtr = addy("61504c");
        int atkPtr = addy("329380");
        int tempPtr = 0;
        int at;

        for (int i=0;i<411;i++,atkPtr+=4,tutorPtr+=4,TMptr+=8){
            currPkmn = new ArrayList<Integer>();
            //get levelup moves
            tempPtr = byteToInt(rom[atkPtr]) + byteToInt(rom[atkPtr+1])*256 + byteToInt(rom[atkPtr+2])*65536;
            at = byteToInt(rom[tempPtr]) +  byteToInt(rom[tempPtr+1])*256;
            while (at != 65535){
                at = at & 511;  //get only the last bit from the attack data
                if (!currPkmn.contains(at)){
                    currPkmn.add(at);
                }
                tempPtr+=2;
                at = byteToInt(rom[tempPtr]) +  byteToInt(rom[tempPtr+1])*256;
            }

            //get TM moves
            int[] TMcompat = new int[]{byteToInt(rom[TMptr]),byteToInt(rom[TMptr+1]),
                    byteToInt(rom[TMptr+2]),byteToInt(rom[TMptr+3]),byteToInt(rom[TMptr+4]),
                    byteToInt(rom[TMptr+5]),byteToInt(rom[TMptr+6]),byteToInt(rom[TMptr+7])};
            int mask = 0;
            for (int p=0;p<TMHMlist.length;p++){
                if (p%8 == 0) mask = 1;
                if ((TMcompat[p/8] & mask) > 0){
                    if (!currPkmn.contains(TMHMlist[p])){
                        currPkmn.add(TMHMlist[p]);
                    }
                }
                mask = mask << 1;
            }

            //get tutor moves
            int[] tutorCompat = new int[]{byteToInt(rom[tutorPtr]),byteToInt(rom[tutorPtr+1]),
                                          byteToInt(rom[tutorPtr+2]),byteToInt(rom[tutorPtr+3])};
            mask = 0;
            for (int p=0;p<tutorList.length;p++){
                if (p%8 == 0) mask = 1;
                if ((tutorCompat[p/8] & mask) > 0){
                    if (!currPkmn.contains(tutorList[p])){
                        currPkmn.add(tutorList[p]);
                    }
                }
                mask = mask << 1;
            }

            //add to out
            out.add(currPkmn);
        }
        return out;
    }

    private ArrayList<ArrayList<Integer>> getPossibleTMTutorAttacks(){
        ArrayList<Integer> currPkmn;
        ArrayList<ArrayList<Integer>> out = new ArrayList<ArrayList<Integer>>();
        int[] TMHMlist = getTMHMlist();
        int[] tutorList = getTutorList();
        int TMptr = addy("31e8a0");
        int tutorPtr = addy("61504c");

        for (int i=0;i<411;i++,tutorPtr+=4,TMptr+=8){
            currPkmn = new ArrayList<Integer>();

            //get TM moves
            int[] TMcompat = new int[]{byteToInt(rom[TMptr]),byteToInt(rom[TMptr+1]),
                    byteToInt(rom[TMptr+2]),byteToInt(rom[TMptr+3]),byteToInt(rom[TMptr+4]),
                    byteToInt(rom[TMptr+5]),byteToInt(rom[TMptr+6]),byteToInt(rom[TMptr+7])};
            int mask = 0;
            for (int p=0;p<TMHMlist.length;p++){
                if (p%8 == 0) mask = 1;
                if ((TMcompat[p/8] & mask) > 0){
                    if (!currPkmn.contains(TMHMlist[p])){
                        currPkmn.add(TMHMlist[p]);
                    }
                }
                mask = mask << 1;
            }

            //get tutor moves
            int[] tutorCompat = new int[]{byteToInt(rom[tutorPtr]),byteToInt(rom[tutorPtr+1]),
                                          byteToInt(rom[tutorPtr+2]),byteToInt(rom[tutorPtr+3])};
            mask = 0;
            for (int p=0;p<tutorList.length;p++){
                if (p%8 == 0) mask = 1;
                if ((tutorCompat[p/8] & mask) > 0){
                    if (!currPkmn.contains(tutorList[p])){
                        currPkmn.add(tutorList[p]);
                    }
                }
                mask = mask << 1;
            }

            //add to out
            out.add(currPkmn);
        }
        return out;
    }

    private int[] getTMHMlist(){
        int[] out = new int[58];
        int TMptr = addy("615b94");
        for (int i=0;i<out.length;i++,TMptr+=2){
            out[i] = byteToInt(rom[TMptr]) + byteToInt(rom[TMptr+1])*256;
        }
        return out;
    }

    private int[] getTutorList(){
        int[] out = new int[30];
        int tutorPtr = addy("61500c");
        for (int i=0;i<out.length;i++,tutorPtr+=2){
            out[i] = byteToInt(rom[tutorPtr]) + byteToInt(rom[tutorPtr+1])*256;
        }
        return out;
    }

    private void writeToROM(int ptr,int[] bytes){
        for (int i=0;i<bytes.length;i++){
            rom[ptr+i] = intToByte(bytes[i]);
        }
    }

    private void changeTradeEvolutions(){
        if (!cbFixEvos.isSelected()) return;
        int ptr = addy("325344");

        //No reqs -- evolve @ L:36
        writeToROM(new int[]{4,0,36,0,65,0,0,0},ptr + 40*63 + 8); //Alakazam
        writeToROM(new int[]{4,0,36,0,68,0,0,0},ptr + 40*66 + 8); //Machamp
        writeToROM(new int[]{4,0,36,0,76,0,0,0},ptr + 40*74 + 8); //Golem
        writeToROM(new int[]{4,0,36,0,94,0,0,0},ptr + 40*92 + 8); //Gengar

        //Metal Coat -- evolve @ L:40
        writeToROM(new int[]{4,0,40,0,208,0,0,0},ptr + 40*94 + 8); //Steelix
        writeToROM(new int[]{4,0,40,0,212,0,0,0},ptr + 40*122 + 8); //Scizor

        //King's Rock -- Moon Stone
        writeToROM(new int[]{7,0,94,0,199,0,0,0},ptr + 40*78 + 16); //Slowking
        writeToROM(new int[]{7,0,94,0,186,0,0,0},ptr + 40*60 + 16); //Politoed

        //Dragon Scale -- evolve @ L:42
        writeToROM(new int[]{4,0,42,0,230,0,0,0},ptr + 40*116 + 8); //Kingdra

        //Upgrade -- evolve @ L:32
        writeToROM(new int[]{4,0,32,0,233,0,0,0},ptr + 40*136 + 8); //Porygon

        //DeepSeaTooth -- low personality
        writeToROM(new int[]{12,0,14,0,118,1,0,0},ptr + 40*372 + 16); //Huntail

        //DeepSeaScale -- high personality
        writeToROM(new int[]{11,0,14,0,119,1,0,0},ptr + 40*372 + 24); //Gorebyss
    }

    private void printStatDocs(){
        String fileLoc = getDocFileName();
        String[] pkmn = ak.getPokemonListGameOrder();
        PrintWriter out;
        String divider = "======================================================================="+
                        "========================================================================";

        try{
            out = new PrintWriter(fileLoc);

            String[] arPkmn = new ArrayKeeper().getPokemonListGameOrder();
            StringBuilder sb;

            //PokeData-----------------------------------------------------------------------
            int ptr = addy("3203e8");
            int[] arData = new int[28];
            PokeData pkdt;

            out.println(divider);
            out.println("   Pokémon Info List");
            out.println(divider);
            out.println(("Pkmn Name      |  HP  AT  DF  SP  SA  SD   |       EPs      |"+
                    " Type(s) |  Ability 1     | Ability 2     | Held Item 1   | Held Item 2   | "));
            out.println(("---------------------------------------------------------"+
                    "----------------------------------------------------------------------------"));

            String[] actualOrder = new String[411];
            for (int i=0;i<411;i++,ptr+=28){
                if (i>=251 && i<276) continue;
                for (int p=0;p<28;p++){
                    arData[p] = byteToInt(rom[ptr+p]);
                }
                sb = new StringBuilder(String.format("%-10s",arPkmn[i]));
                pkdt = new PokeData(arData);
                actualOrder[i] = sb.toString() + " | " + pkdt.toString();
            }
            int[] order = ak.getPkmnDexToGameTranscription();
            for (int i=0;i<order.length;i++){
                out.println(String.format("%03d.",i+1) + actualOrder[order[i]-1]);
            }

            //----------------------------TMs
            out.println("\r\n\r\n" + divider);
            out.println("   TM/HM Compatability List");
            out.println(divider);
            ptr = addy("615b94");
            String[] atList = ak.getAttackListText();
            int tm = 0, tm2 = 0;


            for (int i=0;i<25;i++,ptr+=2){
                tm = byteToInt(rom[ptr]) + byteToInt(rom[ptr+1])*256;
                tm2 = byteToInt(rom[ptr+50]) + byteToInt(rom[ptr+51])*256;
                out.printf("TM%02d - %-10s\tTM%02d - %-10s\r\n",(i+1),atList[tm],(i+26),atList[tm2]);
            }


             //TM compat-----------------------------------------------------------------------------------
            out.println("\r\n\r\n" + divider);
            out.println("   TM/HM Compatability List");
            out.println(divider);
            ptr = addy("31e8a0");
            String header = "\r\n\r\n               |01  03  05|  07  09  |11  13  15|  17  19  |21  23  25|"+
                        "  27  29  |31  33  35|  37  39  |41  43  45|  47  49  |01  03  05|  07  \r\n"+
                            "               |   02  04 |06  08  10|  12  14  |16  18  20|  22  24  |"+
                        "26  28  30|  32  34  |36  38  40|  42  44  |46  48  50|  02  04  |06  08\r\n"+
                            "-----------------------------------------------------------------------"+
                        "------------------------------------------------------------------------";
            int[] TMlist = new int[8];





            actualOrder = new String[411];
            for (int i=0;i<411;i++,ptr+=8){
                sb = new StringBuilder("");
                if (i>=251 && i<276) continue;
                for (int p=0;p<8;p++){      //get TM list
                    TMlist[p] = rom[ptr+p];
                }
                sb.append(String.format("%-10s",arPkmn[i]));
                sb.append(" |");

                int TMptr = 1;
                for (int entry : TMlist){   //print data
                    for (int j=1;j<256;j*=2,TMptr++){
                        if (TMptr >= 58) break;
                        if ((entry & j) > 0){
                            sb.append(" X");
                        } else{
                            sb.append(" -");
                        }
                        if (TMptr % 5 == 0){
                            sb.append("|");
                        }
                    }
                }
                sb.append("\n");
                actualOrder[i] = sb.toString();
            }
            for (int i=0;i<order.length;i++){
                if (i%50 == 0) {            //print header every 50 entries
                    out.println("\n" + header);
                }
                out.println(String.format("%03d.",i+1) + actualOrder[order[i]-1]);
            }

             //Lati@s-----------------------------------------------------------------------------------
            out.println("\r\n\r\n" + divider);
            out.println("   Lati@s choices");
            out.println(divider);
            int latios = byteToInt(rom[addy("242ba7")]) + byteToInt(rom[addy("242ba8")])*256;
            int latias = byteToInt(rom[addy("242bba")]) + byteToInt(rom[addy("242bbb")])*256;
            out.println(decodeText(addy("5ee14b"),3) + "\t" + pkmn[latias-1]);
            out.println(decodeText(addy("5ee14f"),4) + "\t" + pkmn[latios-1]);


            //Pickup Item List------------------------------------------------------------------------------
            out.println("\r\n\r\n" + divider);
            out.println("   Pickup Item List");
            out.println(divider);



        ptr = addy("31c440");
        String[] itemList = ak.getItemList();
        int item;
        String[] pickup = new String[29];
        for (int i=0;i<29;i++,ptr+=2){
            item = byteToInt(rom[ptr]) + byteToInt(rom[ptr+1])*256;
            pickup[i] = itemList[item];
        }
        for (int i=0;i<10;i++){
            out.println("L:" + ((10*i)+1) + "-" + (10*(i+1)));
            out.println("  30% - " + pickup[i]);
            out.print("  10% - " + pickup[i+1]);
            for (int j=2;j<7;j++){
                out.print(", " + pickup[i+j]);
            } out.println("");
            out.println("   5% - " + pickup[i+7]);
            out.println("   3% - " + pickup[i+8]);
            out.println("   1% - " + pickup[i+18] + ", " + pickup[i+19]);
        }


            

            JOptionPane.showMessageDialog(null, "Output file saved successfully!");
            out.close();
        } catch (java.io.FileNotFoundException fnfe){
            JOptionPane.showMessageDialog(null, "Error creating Info Doc file!");
        }
    }

    private void writeAttack(int ptr, int level, int attack){
        rom[ptr+1] = intToByte((level<<1) + attack/256); //replace move at L:1
        rom[ptr] = intToByte(attack%256);
    }

    private void ensureAttacks(int atkPtr, int i){ //i not used -- pokemon index, just in case
        int ptrCopy = atkPtr;
        int[] damagingAttacks = new ArrayKeeper().getAttackListDamaging();
        int[] lvl1Atk = new int[]{byteToInt(rom[atkPtr+1])>>1,(byteToInt(rom[atkPtr+1])%2)*256 + byteToInt(rom[atkPtr])};
        if (lvl1Atk[1] != 144 && indexOf(damagingAttacks,lvl1Atk[1]) == -1){  //144 = Transform
            int[] replacement = new int[2];
            do {
                ptrCopy += 2;
                replacement = new int[]{byteToInt(rom[ptrCopy+1])>>1,(byteToInt(rom[ptrCopy+1])%2)*256 + byteToInt(rom[ptrCopy])};
            } while (replacement[0] != 127 && replacement[1] != 511 && indexOf(damagingAttacks,replacement[1]) == -1);
            if (replacement[0] != 127 && replacement[1] != 511){
//                System.out.println( " >>>>>>>>>" + atkPtr + " " + ptrCopy);
                writeAttack(atkPtr, lvl1Atk[0], replacement[1]); //replace move at L:1
                writeAttack(atkPtr, replacement[0],lvl1Atk[1]);  //swap with replacement
//                rom[ptrCopy+1] = intToByte((replacement[0]<<1) + lvl1Atk[1]/256);        //swap with replacement
//                rom[ptrCopy] = intToByte(lvl1Atk[1]%256);
//                System.out.println("    Pokemon " + i + " -- ptr " + atkPtr + " -- replaced " + lvl1Atk[1] + " with " + replacement[1]);
//                System.out.println(" ~~~~~~~~~~" + replacement[1]/256 + " " + (lvl1Atk[0]<<1) + " = " + rom[atkPtr+1]);
//                System.out.println(" ~~~~~~~~~~" + lvl1Atk[1]/256 + " " + (replacement[0]<<1) + " = " + rom[ptrCopy+1]);
            } else { //no damaging move -- replace L:1 with Tackle
                rom[atkPtr+1] = intToByte(1<<1); //replace move at L:1
                rom[atkPtr] = intToByte(33);
            }
        } else {//it's an attack move
//            System.out.println("    Pokemon " + i + " -- ptr " + atkPtr + " atk " + lvl1Atk[1]);
        }
    }

    private void randomizePokeData(){ // [stats,types,abilities,items]
        boolean[] checks = new boolean[]{cbRandStats.isSelected(),cbRandTypes.isSelected(),
                                    cbRandAbilities.isSelected(),cbRandHeldItems.isSelected(),
                                    cbRandTMLearn.isSelected(),cbMovesets.isSelected(),
                                    cbEnsureAttacks.isSelected()};
        if (!checks[0] && !checks[1] && !checks[2] && !checks[3] && !checks[4] && !checks[5] && !checks[6]){
            return; //nothin' to do here
        }
        
        int ptr = addy("3203e8");
        int TMptr = addy("31e8a0");
        int tutorPtr = addy("61504c");
        int atkPtr = addy("3230dc");
        int[] arData = new int[28];
        PokeData pkdt = new PokeData();
        PokeData oldPkdt = new PokeData();
        boolean useOld = false;
        HashMap<Integer,PokeData> savedPkdt = new HashMap<Integer,PokeData>();
        boolean family = false;
        boolean preevo = false;
        Random rand = new Random();
        int[] hitmontypes = new int[]{0,0};
        int[] preevoArray = ak.getBabies(); //BABIES.
        int preevoPtr = 0;
        int[][] familyTree = ak.getFamilyTree();

        for (int i=0;i<411;i++,ptr+=28,TMptr+=8,tutorPtr+=4){
            if (i>=251 && i<276) {//skip 25 empty slots
                atkPtr += 4;
                continue;
            }
            family = false;
            preevo = false;

            //get new pokemon data
            for (int p=0;p<28;p++){
                arData[p] = byteToInt(rom[ptr+p]);
            }

            //check for family ties
            for (int member : familyTree[i]){
                family = family || member == i; // if member==i, last dex # was a preevolution
            }
            if (family){ //part of continuing family line
                if (i == 133 || i == 236){ //Vaporeon and Hitmontop; should be own new entities
                    savedPkdt.remove(i);
                    useOld = false;
                }else{
                    oldPkdt = pkdt.getClone();
                    useOld = true;
                }
            } else if (savedPkdt.containsKey(i+1)){ //data saved for pokemon from a prior family member
                oldPkdt = savedPkdt.remove(i+1);
                useOld = true;
                //System.out.println("=======> RECALLED: " + (i+1));
            } else{ //new family line
                useOld = false;
            }

            //import new data
            pkdt = new PokeData(arData);

            if (useOld){ //utilize older data --------------------------------------------------------------
                //check if special case for pre-evo
                if (preevoPtr < preevoArray.length && preevoArray[preevoPtr] == i){
                    preevo = true;
                    preevoPtr += 1;
                } else{
                    preevo = false;
                }

                //stats
                if (checks[0]){
                    pkdt.swapStats(oldPkdt.getStatSwaps());
                }

                //types
                if (checks[1]){
                    int[] oldTypes = oldPkdt.getTypes();
                    pkdt.setTypes(oldTypes[0], oldTypes[1]);

                    if (!preevo){
                        pkdt.rerandomizeTypes();
                    } else{
                        pkdt.derandomizeTypes();
                    }
                }

                //abilities
                if (checks[2]){
                    int[] oldAbils = oldPkdt.getAbilities();
                    pkdt.setAbilities(oldAbils[0], oldAbils[1]);
                }

                //items
                if (checks[3]){
                    int[] oldItems = oldPkdt.getItems();
                    pkdt.setHeldItems(oldItems[0], oldItems[1]);

                    if (oldItems[0] == 0 || oldItems[1] == 0){
                        if (!preevo){
                            pkdt.rerandomizeItems();
                        } else{
                            pkdt.derandomizeItems();
                        }
                    } else{ //both item slots are full
                        if (preevo){
                            pkdt.derandomizeItems();
                        }
                    }
                }

                //TM compatability
                if (checks[4]){
                    pkdt.setTMCompatability(oldPkdt.getTMCompatability());
                    pkdt.setTutorCompatability(oldPkdt.getTutorCompatability());
                    if (preevo){
                        pkdt.derandomizeTMCompatability();
                        pkdt.derandomizeTutorCompatability();
                    } else{
                        pkdt.rerandomizeTMCompatability();
                        pkdt.rerandomizeTutorCompatability();
                    }
                }

                //Attacks
                if (checks[5]){
                    ArrayList<Integer> movelist = oldPkdt.getAvailableMoves();
                    HashMap<Integer,Integer> atkHash = oldPkdt.getAttackHash();
                    if (movelist == null){
                        movelist = ak.getArrayListInt(ak.getAttackList());
                    }
                    if (atkHash == null){
                        atkHash = new HashMap<Integer,Integer>();
                    }
                    int oldAtk = 1;
                    int lvl = 1;
                    int atk = 0;
                    if (checks[6]){ //ensure first move is an attack!
                        //note that ALL pokemon have at least 1 move, so this should be executed alright
                        lvl = byteToInt(rom[atkPtr+1])>>1;
                        oldAtk = (byteToInt(rom[atkPtr+1])%2)*256 + byteToInt(rom[atkPtr]);
                        if (atkHash.containsKey(oldAtk)){
                            atk = atkHash.get(oldAtk);
                        } else{
                            int[] dmgAtkList = ak.getAttackListDamaging();
                            int index = rand.nextInt(dmgAtkList.length);
                            atk = dmgAtkList[index];
                            while (!movelist.contains(atk)){
                                index++;
                                atk = dmgAtkList[index];
                            }
                            atk = movelist.remove(movelist.indexOf(atk));
                            atkHash.put(oldAtk, atk);
                        }
                        writeAttack(atkPtr,lvl,atk);
                        atkPtr += 2;
                    }

                    //main attack loop
                    while (byteToInt(rom[atkPtr])!=255 || byteToInt(rom[atkPtr+1])!=255){
                        lvl = byteToInt(rom[atkPtr+1])>>1;
                        oldAtk = (byteToInt(rom[atkPtr+1])%2)*256 + byteToInt(rom[atkPtr]);
                        if (atkHash.containsKey(oldAtk)){ //already on movelist
                            atk = atkHash.get(oldAtk);
                        } else{
                            atk = movelist.remove(rand.nextInt(movelist.size()));
                            atkHash.put(oldAtk, atk);
                        }
                        writeAttack(atkPtr,lvl,atk);
                        atkPtr += 2;
                    }
                    atkPtr += 2;    //pass final FFFF
                    pkdt.setAvailableMoves(movelist);
                    pkdt.setAttackHash(atkHash);
                } else if (checks[6]){ //ensure attacks @ L:1
                    ensureAttacks(atkPtr,i);
                    while (byteToInt(rom[atkPtr])!=255 || byteToInt(rom[atkPtr+1])!=255){
                        atkPtr += 2;
                    } atkPtr +=2;   //pass final FFFF
                }
            } else{ //new  ----------------------------------------------------------------------------
                //stats
                if (checks[0]){
                    pkdt.randomizeStats();
                }

                //types
                if (checks[1]){
                    pkdt.randomizeTypes();
                }

                //abilities
                if (checks[2]){
                    pkdt.randomizeAbilities();
                }

                //items
                if (checks[3]){
                    pkdt.randomizeItems();
                }

                //TM compatability
                if (checks[4]){
                    if (i==150){ //Mew retains its ability to learn all TMs
                        pkdt.setTMCompatability(new int[]{255,255,255,255,255,255,255,255});
                        pkdt.setTutorCompatability(new int[]{255,255,255,255});
                    } else{
                        pkdt.randomizeTMCompatability();
                        pkdt.randomizeTutorCompatability();
                    }
                }

                //Attacks
                if (checks[5]){
                    ArrayList<Integer> movelist = ak.getArrayListInt(ak.getAttackList());
                    HashMap<Integer,Integer> atkHash = new HashMap<Integer,Integer>();
                    int oldAtk = 1;
                    int lvl = 1;
                    int atk = 1;
                    if (checks[6]){ //ensure first move is an attack!
                        //note that ALL pokemon have at least 1 move, so this should be executed alright
                        int[] dmgAtkList = ak.getAttackListDamaging();
                        lvl = byteToInt(rom[atkPtr+1])>>1;
                        oldAtk = (byteToInt(rom[atkPtr+1])%2)*256 + byteToInt(rom[atkPtr]);
                        atk = dmgAtkList[rand.nextInt(dmgAtkList.length)];
                        if (movelist.contains(atk)){ //...which it should
                            movelist.remove(movelist.indexOf(atk));
                        }
                        atkHash.put(oldAtk, atk);
                        writeAttack(atkPtr,lvl,atk);
                        atkPtr += 2;
                    }

                    //main attack loop
                    while (byteToInt(rom[atkPtr])!=255 || byteToInt(rom[atkPtr+1])!=255){
                        lvl = byteToInt(rom[atkPtr+1])>>1;
                        oldAtk = (byteToInt(rom[atkPtr+1])%2)*256 + byteToInt(rom[atkPtr]);
                        if (atkHash.containsKey(oldAtk)){ //already on movelist
                            atk = atkHash.get(oldAtk);
                        } else{
                            atk = movelist.remove(rand.nextInt(movelist.size()));
                            atkHash.put(oldAtk, atk);
                        }
                        writeAttack(atkPtr,lvl,atk);
                        atkPtr += 2;
                    }
                    atkPtr += 2;    //pass final FFFF
                    pkdt.setAvailableMoves(movelist);
                    pkdt.setAttackHash(atkHash);
                } else if (checks[6]){ //ensure attacks @ L:1
                    ensureAttacks(atkPtr,i);
                    while (byteToInt(rom[atkPtr])!=255 || byteToInt(rom[atkPtr+1])!=255){
                        atkPtr += 2;
                    } atkPtr +=2;   //pass final FFFF
                }
            }

            //change for hitmon cases
            if (i==106 || i==235 || i==236){
                pkdt.setTypes(hitmontypes[0], hitmontypes[1]);
            }

            //change for castform
            if (i==384) {
                pkdt.setAbility(0,59);
                pkdt.setAbility(1,0);
            }

            //change for Shedinja
            if (i==302) {
                pkdt.setAbility(0,25);
                pkdt.setAbility(1,0);
            }

            //apply changes------------------------------------------
            int evs = 0;

            if (checks[0]){
                for (int p=0;p<6;p++){  //fill in stats (hp/at/df/sp/sa/sd)
                    rom[ptr+p] = intToByte(pkdt.getStat(p));
                    evs += pkdt.getEV(p) << p*2;
                }
                rom[ptr+10] = intToByte(evs % 256);
                rom[ptr+11] = intToByte(evs / 256);
            }

            if (checks[1]){
                rom[ptr+6] = intToByte(pkdt.getType(0));
                rom[ptr+7] = intToByte(pkdt.getType(1));
            }

            if (checks[2]){
                rom[ptr+22] = intToByte(pkdt.getAbility(0));
                rom[ptr+23] = intToByte(pkdt.getAbility(1));
            }

            if (checks[3]){
                for (int p=0;p<4;p+=2){
                    int item = pkdt.getItem(p/2);
                    rom[ptr+13+p] = intToByte(item / 256);
                    rom[ptr+12+p] = intToByte(item % 256);
                }
            }

            if (checks[4]){
               int[] TMCompat = pkdt.getTMCompatability();
                for (int p=0;p<TMCompat.length;p++){
                    rom[TMptr+p] = intToByte(TMCompat[p]);
                }
               int[] tutorCompat = pkdt.getTutorCompatability();
                for (int p=0;p<tutorCompat.length;p++){
                    rom[tutorPtr+p] = intToByte(tutorCompat[p]);
                }
            }

            //save data, if necessary, for future family members
            if (familyTree[i].length > 0){
                ArrayList<Integer> alFam = new ArrayList<Integer>();
                int highest = i+1;
                for (int member : familyTree[i]){
                    alFam.add(member);
                    highest = Math.max(highest, member);
                }

                //remove all consecutive evolutions
                for (int k=2;alFam.contains(i+k);k++){  //2 because it's +1 (i.e. Bulbasaur's i is 0, dex is 1)
                    alFam.remove(alFam.indexOf(i+k));
                }

                //remove all consecutive pre-evolutions
                for (int k=0;alFam.contains(i-k);k++){
                    alFam.remove(alFam.indexOf(i-k));
                }

                if (i == 289){ //Wurmple
                    savedPkdt.put(293, pkdt.getClone());
                }

                if (alFam.size() > 0){ //there a nonconsecutive family member
                    if (highest != i+1) { //only put it in if we need to
                        savedPkdt.put(highest, pkdt.getClone());
                        //System.out.println(" <> Stored family pokedata: " + (i+1));
                    }
                }
                if (i==105) { //hitmonlee
                    hitmontypes = new int[]{pkdt.getType(0),pkdt.getType(1)};
                }
            }
        }
    }

    private void randomizePickup(){
        if (!cbRandPickup.isSelected()) return;
        int ptr = addy("31c440");
        ArrayKeeper ark = new ArrayKeeper();
        ArrayList<Integer> itemList = ark.getArrayListInt(ark.getUsableItems());
        int item;
        Random rand = new Random();
        for (int i=0;i<29;i++,ptr+=2){
            item = itemList.remove(rand.nextInt(itemList.size()));
            rom[ptr] = intToByte(item % 256);
            rom[ptr+1] = intToByte(item / 256);
        }
    }

    private void testPalettes(){
        try{
            File out = new File("paletteTest.bmp");
            BufferedImage bi = new BufferedImage(325,289,BufferedImage.TYPE_INT_RGB);
            Graphics2D gr = bi.createGraphics();

            int ptry = 1;
            int ptrx = 1;
            int placex = 0;
            int placey = 0;
            PokePalette pp = new PokePalette();
            for (int[] col : new ArrayKeeper().getBaseColors()){
                for (int i=3;i<7;i+=2){
                    
                    if (i==3) {
                        ptry = 1 + 6*8*placey;
                    }

                    ptrx = 1 + 6*6*placex;
                    int[][] currCol = pp.getOddColors(i,col,false,false,false,false);

                    for (int[] cCol : currCol){
//                        System.out.println(cCol[0] + " " + cCol[1] + " " + cCol[2]);
                        gr.setColor(new Color(cCol[0],cCol[1],cCol[2]));
                        gr.fill(new Rectangle(ptrx, ptry, 5, 5));
                        ptrx += 6;
                    }
                    ptry += 6;

                    ptrx = 1+ 6*6*placex;
                    currCol = pp.getOddColors(i,col,true,false,false,false);

                    for (int[] cCol : currCol){
//                        System.out.println(cCol[0] + " " + cCol[1] + " " + cCol[2]);
                        gr.setColor(new Color(cCol[0],cCol[1],cCol[2]));
                        gr.fill(new Rectangle(ptrx, ptry, 5, 5));
                        ptrx += 6;
                    }
                    ptry += 6;

                    ptrx = 1+ 6*6*placex;
                    currCol = pp.getEvenColors(i+1,col,false,false,false,false);
                    for (int[] cCol : currCol){
//                        System.out.println(cCol[0] + " " + cCol[1] + " " + cCol[2]);
                        gr.setColor(new Color(cCol[0],cCol[1],cCol[2]));
                        gr.fill(new Rectangle(ptrx, ptry, 5, 5));
                        ptrx += 6;
                    }
                    ptry += 6;

                    ptrx = 1+ 6*6*placex;
                    currCol = pp.getEvenColors(i+1,col,true,false,false,false);
                   
                    /*if (i==3){
                        test = "1,2,3,4"; pl = 3;
                    } else {
                        test = "1,2,3,4,5,6"; pl = 4;
                    }
                    currCol = makeSiblingPalette(col,test,pl,true);*/

                    for (int[] cCol : currCol){
//                        System.out.println(cCol[0] + " " + cCol[1] + " " + cCol[2]);
                        gr.setColor(new Color(cCol[0],cCol[1],cCol[2]));
                        gr.fill(new Rectangle(ptrx, ptry, 5, 5));
                        ptrx += 6;
                    }
                    ptry += 6;
                }
                placey++;
                if (placey >= 6){
                    placey = 0;
                    placex++;
                }
            }

            ImageIO.write(bi, "bmp", out);
            System.out.println("Image written!");
        } catch (IOException ioe){
            System.out.println("I/O Exception! Yoiks!");
        }
    }

    private void changePkmnColors(){
        if (rbPalNoChange.isSelected()) return;

        //Single Family Palette
        int prevPalChance;
        if (cbKeepColors.isSelected()){
            prevPalChance = 100;
        } else{
            prevPalChance = PREV_PAL_CHANCE;
        }

        //change only primary color
        int colorDataLimit;
        if (cbPalPrimary.isSelected()){
            if (rbPalType.isSelected()){
                colorDataLimit = 2;  //2 => one for each type
            } else {
                colorDataLimit = 1;  //2 => one for each type
            }
        } else{
            colorDataLimit = 16; //max colors is 16 due to palettes being 16 colors
        }

        //load the palette data from the palette file
        String[] palData = new String[388];
        InputStream infi = getClass().getResourceAsStream("resources/palettes.txt");
        BufferedReader br = new BufferedReader(new InputStreamReader(infi));
        String line;
        int ptr = 0;
        int[][] familyTree = new ArrayKeeper().getFamilyTree();

        //read palette data in
        try{
            while ((line = br.readLine()) != null && ptr < palData.length){
                palData[ptr] = line.trim();
                ptr++;
            }
        } catch (java.io.IOException ioe){
            JOptionPane.showMessageDialog(null,"An I/O Exception has occurred during palette file parsing.");
        }

        //change palettes
        ptr = addy("c2fca0");                   //starting point for palettes - d30024 for R/S, c2fca0 for E
        int pkdtPtr = addy("3203e8");           //pokemon data start (for types) - 1febc4 for R/S, 3203e8 for E
        //ptr = addy("dddcec");
        Random rand = new Random();             //pseudorandom number gen
        String[] parts = new String[0];         //values from splitting slashes
        HashMap<Integer,PokePalette> pkmnRecall = new HashMap<Integer,PokePalette>();   //recall prev. palettes
        int pkmnIndex;                          //index for some arrays/lists (due to some special case skipping)
        PokePalette palStruct = new PokePalette();
        int[] origPalData = new int[44];
        boolean shiny = cbShinyNorm.isSelected();
        int ptrBeginning = 0;                   //starting place for ptr to help find how many bytes were used
        int ptrEnd = 0;                         //ending point for ptr

        //start at 0
         for (int i=0;i<412;i++){//412
            
            if (i==251) { //need to skip palette data for Missingno. (and Castform?)
                ptr = movePtrToNextPalette(ptr,3); //move twice to get rid of palette and shiny palette
                continue;
            } else if ((i>251 && i<277) || i==385){ //25 blank slots, Castform
                pkdtPtr+=28;
                continue;
            }

            pkmnIndex = i - i/251; //252= Missingno.; 360 = Castform (+25)

            //copy original palette data if shinies are to have normal colors
            if (shiny){
                ptrBeginning = ptr;
                for (int p=0;p<origPalData.length;p++){
                    origPalData[p] = byteToInt(rom[ptr+p]);
                }
            }

//            System.out.println("--------------POKEMON "+(pkmnIndex+1) + " -- LOOP " + i + "-----------------");

            //handles base palette reloading or conservation-----------------
            boolean family = false;
//            System.out.println(familyTree[pkmnIndex].length);
            for (int member : familyTree[pkmnIndex]){
                family = family || member == pkmnIndex; // if member==i, last dex # was a preevolution
                //if (member > pkmnIndex+1) continue;
            }
            if (family){ //part of continuing family line
                if (rand.nextInt(100) > prevPalChance){ //new palette!
                    if (pkmnIndex == 327){ //327 = Shedinja
                        palStruct.resetColorsFamily();
                    } else{
                        palStruct.resetColors();
                    }
                } else{ //old palette is fine
                    if (pkmnIndex == 133 || pkmnIndex == 236){ //133 = Vaporeon, 236 = Hitmontop
                        palStruct.resetColors();
                    } else{
                        palStruct.resetColorsFamily();
                    }
                }
            } else{ //new family line
                if (pkmnRecall.containsKey(pkmnIndex+1)){ //it has a saved palette from an earlier pokemon
                    palStruct.resetColors();
                    if (i != 235){ //Tyrogue, prevents it from getting Hitmonchan's data
                        palStruct = pkmnRecall.remove(pkmnIndex+1);
                    }
//                    System.out.println("=======> RECALLED: " + (pkmnIndex+1));
                } else{ //new pokemon altogether
                    palStruct.resetColors();
                }
            }
            //fill required colors into base color--------------------------------------
            //get the palette data from the file
            //System.out.println(palData[i]);
            parts = palData[i-25*(i/251)].split("/");
            if (parts.length == 0) continue; //skip Missingno. and Castform
            //fullPalette = new String[16];  //full palette, as the game would see it

            if (rbPalType.isSelected()){
                int type1 = byteToInt(rom[pkdtPtr+6]);
                int type2 = byteToInt(rom[pkdtPtr+7]);
//                System.out.println("TYPES: " + type1 + " " + type2);
                palStruct.fillBaseColorsByType(parts.length,type1,type2);
            } else{
                palStruct.fillBaseColors(parts.length);
            }

            //if pokemon has nonconsecutive pre- or post-evo, save its palette data---------------
            if (familyTree[pkmnIndex].length > 0){
                ArrayList<Integer> alFam = new ArrayList<Integer>();
                int highest = pkmnIndex;
                for (int member : familyTree[pkmnIndex]){
                    alFam.add(member);
                    highest = Math.max(highest, member);
                }
                //remove all consecutive evolutions
                for (int k=2;alFam.contains(pkmnIndex+k);k++){  //2 because it's +1 (i.e. Bulbasaur's i is 0, dex is 1)
                    alFam.remove(alFam.indexOf(pkmnIndex+k));
                }
                //remove all consecutive pre-evolutions
                for (int k=0;alFam.contains(pkmnIndex-k);k++){
                    alFam.remove(alFam.indexOf(pkmnIndex-k));
                }
                if (alFam.size() > 0){ //there a nonconsecutive family member
                    if (highest != pkmnIndex) { //only put it in if we need to
                        pkmnRecall.put(highest, (PokePalette)palStruct.getClone());
//                        System.out.println(" <> Stored family palette data: " + (pkmnIndex+1));
                    }
                }
            } //colors chosen. Make gradients to fill in-game 16-slot palette----------------------

            for (int p=0;p<parts.length && p<colorDataLimit;p++){
                //System.out.print("  > pal part " + p + " : ");
                //palStruct.print();

                String colorData = parts[p];//color data
                int sharedColor = -1;       //color siblings share
                String sibling = "none";    //sibling color data
                boolean endDarkened = false;

                //sort extra data
                while (colorData.contains("-")){
                    if (colorData.endsWith("L")){
                        palStruct.setLight(p);//palStruct.getLightDark().size()
                        colorData = colorData.substring(0,colorData.lastIndexOf("-"));
                    } else if (colorData.endsWith("D")){
                        palStruct.setDark(p);//palStruct.getLightDark().size()
                        colorData = colorData.substring(0,colorData.lastIndexOf("-"));
                    } else if (colorData.endsWith("LN")){ //not light
                        palStruct.ensureNotLight(p);
                        colorData = colorData.substring(0,colorData.lastIndexOf("-"));
                    } else if (colorData.endsWith("DN")){ //not dark
                        palStruct.ensureNotDark(p);
                        colorData = colorData.substring(0,colorData.lastIndexOf("-"));
                    } else if (colorData.endsWith("B")){ //base only
                        palStruct.ensureBase(p);
                        colorData = colorData.substring(0,colorData.lastIndexOf("-"));
                    } else if(colorData.endsWith("E")){ //make end color darker
                        endDarkened = true;
                        colorData = colorData.substring(0,colorData.lastIndexOf("-"));
                    } else {
                        //when it does, return the list WITH the duplicate as well -- appended
                        if (colorData.indexOf(";")==-1){
                            //textSentinel
                            System.out.println("Error color input with hyphen usage.\n\t"+colorData);
                            return;
                        }
                        sharedColor = Integer.parseInt(colorData.substring(colorData.lastIndexOf("-")+1));
                        colorData = colorData.substring(0,colorData.lastIndexOf("-"));
                        String[] halves = colorData.split(";");
                        try{
                            sibling = halves[1];
                            colorData = halves[0];
                        } catch (ArrayIndexOutOfBoundsException aioobe){
                            //textSentinel
                            System.out.println("Error referencing halves array.\n\t"+parts[p]);
                        }
                    }
                }
                if (parts[p].length() == 0){ //color skipped to make colors line up between family members
                    continue;
                }

                //fill palette
                if (sibling.equals("none")){ //normal color
                    palStruct.fillPaletteSlots(colorData,p,endDarkened);
                } else{ //sibling palette
                    palStruct.fillPaletteSlotsSibling(colorData,sibling,sharedColor,p,endDarkened);
                }
            }

            /*System.out.println("Palette:");
            palStruct.print();*/
            //palStruct.printColorUsed();
            /*ArrayList<Integer> ar = palStruct.getLightDark();
            for (int j=0;j<ar.size();j++){
                System.out.print(ar.get(j) + ", ");
            } System.out.println("");*/


            //palStruct.print();
            // palettes ready to transfer----------------------------------------------
            //debug = ptr;
            //System.out.println("-------orig palette:");
            //printDebug(debug);

            ptr += 4; //skip header of 10200000
            int blockCode = byteToInt(rom[ptr]);
            ptr++;

            int carryoverByte = -1;
            int colorPtr = 0;
            boolean skippedCol = false; //handle second byte of color if it is skipped
            while (colorPtr < 16){
                if (blockCode == 0){
                  if (carryoverByte == -1 && skippedCol == false){ // it had no oddities:
                      for (int j=4;j>0 && colorPtr < 16;j--, colorPtr++, ptr += 2){
                          if (palStruct.getColor(colorPtr).equals("null")) continue; //no palette change
                          rom[ptr] = intToByte(hexToInt(palStruct.getColor(colorPtr).substring(0,2)));
                          rom[ptr+1] = intToByte(hexToInt(palStruct.getColor(colorPtr).substring(2)));
                      }
                  } else {
                      //System.out.println(" ~~ carryover ~~  ptr = " + intToHex(ptr,8));
                      for (int j=0;j<4 && colorPtr < 16 ;j++, colorPtr++, ptr += 2){
                          //System.out.println(" <> " + j + " " + colorPtr);
                          if (carryoverByte != -1){
                              rom[ptr] = intToByte(carryoverByte);
                          }
                          if (skippedCol){
                              skippedCol = false;
                              //ptr++;
                          }
                          if (palStruct.getColor(colorPtr).equals("null")) {//no palette change
                              carryoverByte = -1;
                              skippedCol = true;
                              continue;
                          }
                          rom[ptr+1] = intToByte(hexToInt(palStruct.getColor(colorPtr).substring(0,2)));
                          carryoverByte = intToByte(hexToInt(palStruct.getColor(colorPtr).substring(2)));
                      }
                      if (colorPtr >= 16 && carryoverByte != -1){ //handle remaining carryover if off by 1
                          rom[ptr] = intToByte(carryoverByte);
                          ptr++;
                      }
                  }
                } else { //block code != 0, there are some special instructions mixed in
//                    int inst = 7; //get the location for the instruction (1001, 3001, etc.)
                    int focus = 256;
                    for (int p=0;p<8 && colorPtr < 16;p++,ptr++){
                        focus = focus >> 1;
//                        System.out.println("blockCode: " + intToHex(blockCode,2) + " -- colorPtr: " + colorPtr);
                        if ((blockCode & focus) > 0){ //special code
                            int[] breakBytes = {byteToInt(rom[ptr]), byteToInt(rom[ptr+1])};
//                            System.out.println("Break bytes: "+intToHex(breakBytes[0],2) + " " + intToHex(breakBytes[1],2) + " ptr: " + intToHex(ptr,8));
                            ptr ++;

                            if (breakBytes[1] == 1){ //normal
                                colorPtr += (int)(((breakBytes[0]/16) * 0.5) + 1.5); // y = 0.5 * x + 1.5 -- (1,2)(3,3)(5,4), etc.
                                //System.out.println("Skipped " + (int)(((breakBytes[0]/16) * 0.5) + 1.5) + " at: " + colorPtr);
                            } else{ //special cases
                                //System.out.println(" POINTER: " + intToHex(ptr,8) + " " + rom[ptr] + " " + rom[ptr+1]);
                                int reps = (int)(((breakBytes[0]/16) * 0.5) + 1.5); //not used?
                                switch (breakBytes[1]){
                                    case 3: //assumes color slot 0, then byte FF (equivalent to 3 bytes: first color, then 0xFF)
                                        // this does count as the first half of the next color
                                        colorPtr++; //color slot 0 copy
                                         if (!palStruct.getColor(colorPtr).equals("null")){ //Kangaskhan
                                             //rom[ptr] = intToByte(hexToInt(palStruct.getColor(colorPtr).substring(2)));
                                             carryoverByte = intToByte(hexToInt(palStruct.getColor(colorPtr).substring(2)));
                                         } else { //Igglybuff
                                            //bytesToGo++;
//                                            ptr--;
                                         }
                                        colorPtr++;
//                                        ptr++;
                                        //bytesToGo--;
                                        break;
                                    case 5: case 7: case 9: //next 2 are copied
                                        colorPtr += 2;
                                        break;
                                    case 11: //2 additional skipped colors
                                        colorPtr += 2;
                                        break;
                                    case 15: //next 3 are skipped
                                        colorPtr += 3;
                                        break;
                                    case 17: //next 4 are skipped
                                        colorPtr += 4;
                                        break;
                                    default:
//                                        System.out.println(" !!!!!! > NEW SPECIAL CASE: " + breakBytes[0] + " " + breakBytes[1]);
                                }
                            }
                        } else{ //nothing -- color data as usual
                            if (carryoverByte != -1){
                                rom[ptr] = intToByte(carryoverByte);
                                carryoverByte = -1;
                            } else{
                                if (skippedCol) {//add 1 to ptr for second byte of skipped color
                                    skippedCol = false;
                                    continue;
                                }
                                if (palStruct.getColor(colorPtr).equals("null")) {//no palette change
                                    carryoverByte = -1;
                                    skippedCol = true;
                                    colorPtr++;
                                    continue;
                                } //otherwise, put new color in
                                rom[ptr] = intToByte(hexToInt(palStruct.getColor(colorPtr).substring(0,2)));
                                carryoverByte = intToByte(hexToInt(palStruct.getColor(colorPtr).substring(2)));
                                colorPtr++;
                            }
                        }
//                        blockCode = blockCode >> 1; //shift to the next piece of blockCode
                    }




 /*                   int inst = 7; //get the location for the instruction (1001, 3001, etc.)
                    for (; blockCode % 2 == 0; inst --){
                        blockCode = blockCode >> 1;
                    }
                    int bytesToGo = 8; //7 from colors, 2 from control code
                    //System.out.println("break at: " + inst);
                    //System.out.println("colorPtr: " + colorPtr);
                    //cover colors until break

                    for (int j=0;j<inst && colorPtr<16;j++, ptr++, bytesToGo--){
                        if (carryoverByte != -1){
                            rom[ptr] = intToByte(carryoverByte);
                            carryoverByte = -1;
                        } else{
                            if (skippedCol) {//add 1 to ptr for second byte of skipped color
                                skippedCol = false;
                                continue;
                            }
                            if (palStruct.getColor(colorPtr).equals("null")) {//no palette change
                                carryoverByte = -1;
                                skippedCol = true;
                                colorPtr++;
                                continue;
                            }
                            rom[ptr] = intToByte(hexToInt(palStruct.getColor(colorPtr).substring(0,2)));
                            carryoverByte = intToByte(hexToInt(palStruct.getColor(colorPtr).substring(2)));
                            colorPtr++;
                        }
                    }

                    //grab break bytes and apply effect ----------
                    int[] breakBytes = {byteToInt(rom[ptr]), byteToInt(rom[ptr+1])};
                    //System.out.println("Break bytes: "+breakBytes[0] + " " + breakBytes[1] + " ptr: " + intToHex(ptr,8));
                    ptr += 2;
                    bytesToGo --; //only count as one byte used

                    if (breakBytes[1] == 1){ //normal
                        colorPtr += (int)(((breakBytes[0]/16) * 0.5) + 1.5); // y = 0.5 * x + 1.5 -- (1,2)(3,3)(5,4), etc.
                        //System.out.println("Skipped " + (int)(((breakBytes[0]/16) * 0.5) + 1.5) + " at: " + colorPtr);
                    } else{ //special cases
                        //System.out.println(" POINTER: " + intToHex(ptr,8) + " " + rom[ptr] + " " + rom[ptr+1]);
                        int reps = (int)(((breakBytes[0]/16) * 0.5) + 1.5); //not used?
                        switch (breakBytes[1]){
                            case 3: //assumes color slot 0, then byte FF (equivalent to 3 bytes: first color, then 0xFF)
                                // this does count as the first half of the next color
                                colorPtr += 1;
                                 if (!palStruct.getColor(colorPtr).equals("null")){ //Kangaskhan
                                     rom[ptr] = intToByte(hexToInt(palStruct.getColor(colorPtr).substring(2)));
                                 } else { //Igglybuff
                                    bytesToGo++;
                                    ptr--;
                                 }
                                colorPtr++;
                                ptr++;
                                bytesToGo--;
                                break;
                            case 5: case 7: case 9: //next 2 are copied
                                colorPtr += 2;
                                break;
                            case 11: //2 additional skipped colors
                                colorPtr += 2;
                                break;
                            case 15: //next 3 are skipped
                                colorPtr += 3;
                                break;
                            case 17: //next 4 are skipped
                                colorPtr += 4;
                                break;
                            default:
                                System.out.println(" !!!!!! > NEW SPECIAL CASE: " + breakBytes[0] + " " + breakBytes[1]);
                        }
                    }

                    //finish putting in colors ------------------
                    //System.out.println(" Color Ptr/carry/skip/togo -- "
                    //        + colorPtr + " " + carryoverByte + " " + skippedCol + " " + bytesToGo);
                    for (;bytesToGo > 0 && colorPtr < 16;ptr++,bytesToGo--){
                        if (carryoverByte != -1){
                            rom[ptr] = intToByte(carryoverByte);
                            carryoverByte = -1;
                        } else{
                            if (skippedCol) {//add 1 to ptr for second byte of skipped color
                                skippedCol = false;
                                continue;
                            }
                            if (palStruct.getColor(colorPtr).equals("null")) {//no palette change
                                carryoverByte = -1;
                                skippedCol = true;
                                colorPtr++;
                                continue;
                            }
                            rom[ptr] = intToByte(hexToInt(palStruct.getColor(colorPtr).substring(0,2)));
                            carryoverByte = intToByte(hexToInt(palStruct.getColor(colorPtr).substring(2)));
                            colorPtr++;
                        }
                    }
                    //System.out.println(" ~~ Color Ptr/carry/skip/togo -- "
                    //        + colorPtr + " " + carryoverByte + " " + skippedCol + " " + bytesToGo);

*/
                }
                blockCode = byteToInt(rom[ptr]);
                ptr++;
            }
            //System.out.println("-----------finished output:");
            //printDebug(debug);

            //replace shiny palette data
            if (shiny){

                ptrEnd = ptr-1;
                ptr = movePtrToNextPalette(ptr,1);
                
                //replace shiny data with original colors
                if (i == 356){ //meditite exception -- there's too many bytes to skip...
                    byte[] pal = hexStringToByteArray("1020000000F146FF7F9D73196300F23D787FF17A4C6600C95569390000787F20F17A300B");
                    for (int p=0;p<pal.length;p++){
                        rom[ptr+p] = pal[p];
                    }
                } else{
                    for (int p=0;p<ptrEnd-ptrBeginning;p++){
                        rom[ptr+p] = intToByte(origPalData[p]);
                    }
                    //catch last byte, if necessary
                    int mod = 0;
                    for (int p=ptrEnd-ptrBeginning;p>origPalData.length || origPalData[p] != 16;p++,mod++){
    //                    System.out.println("  --> " + origPalData[p]);
                        rom[ptr+p] = intToByte(origPalData[p]);
                    }
                    //get rid of excess info
                    for (int p=ptrEnd-ptrBeginning+mod;p<origPalData.length;p++){
                        rom[ptr+p] = intToByte(0);
                    }
                }
                
                ptr++;
                ptr = movePtrToNextPalette(ptr,1);
            } else{
                //get to next piece of palette data
                ptr = movePtrToNextPalette(ptr,2);
            }
           //get to next piece of Poke Data
            pkdtPtr+=28;
         }

        //Castform
        String[] castform = new String[]{"5,4,3,13,2/7,8-L","5,4,3,2/7,8-L/9,10,11",   //normal, sunny
                            "5,4,3,2/7,8-L/9,10,11","4,3,11/7,8-L/12,9,10,13,2"}; //rain, hail
        ptr = addy("d28708");       //R/S: e27c70; E: d28708
        pkdtPtr = addy("322de8");   //R/S: 2015c4; E: 322de8
        if (shiny){
            origPalData = new int[128];
            for (int p=0;p<128;p++){
                origPalData[p] = byteToInt(rom[ptr+p]);
            }
        }

        int onByte = 8;
        ptr += 4; //skip header of 10800000
        int blockCode = 0;
        int[] breakBytes = new int[]{0,0};
        int focus = 128;
        int type1,type2;

        for (int r=0;r<4;r++){
//            System.out.println("Castform " + r + " changed!");
            palStruct.resetColors();
            parts = castform[r].split("/");

            if (rbPalType.isSelected()){
                if (r == 0){        //normal
                    type1 = byteToInt(rom[pkdtPtr+6]);
                    type2 = byteToInt(rom[pkdtPtr+7]);
                } else if (r == 1){ //sunny
                    type1 = 10; //FIR
                    type2 = 10;
                } else if (r == 2){ //rain
                    type1 = 11; //WTR
                    type2 = 11;
                } else{             //hail
                    type1 = 15; //ICE
                    type2 = 15;
                }
                
                palStruct.fillBaseColorsByType(parts.length,type1,type2);
            } else{
                palStruct.fillBaseColors(parts.length);
            }

            //put color data into rom
            for (int p=0;p<parts.length && p<colorDataLimit;p++){
                String colorData = parts[p];    //color data

                //sort extra data
                while (colorData.contains("-")){  //light is the only one Castform has
                    palStruct.setLight(p);
                    colorData = colorData.substring(0,colorData.lastIndexOf("-"));
                }
                //fill palette
                palStruct.fillPaletteSlots(colorData,p,false);
            }
//            palStruct.print();

            int carryoverByte = -1;
            int colorPtr = r/2;     //normal and sunny start at color 0; others at color 1
            boolean skippedCol = false;

            while ((colorPtr < 16 || skippedCol)){
                if (onByte >= 8){
                    blockCode = byteToInt(rom[ptr]);
                    ptr++;
                    onByte = 0;
                    focus = 128;
//                    System.out.println(" --- recharged! -- " + blockCode + " -- ptr: " + intToHex(ptr,8));
                }

                if ((blockCode & focus) > 0){ //special case
//                    System.out.println(" > special: " + blockCode + " -- ptr: " + intToHex(ptr,8));
                    breakBytes = new int[]{byteToInt(rom[ptr]), byteToInt(rom[ptr+1])};
//                    System.out.println(" ... break bytes: " + intToHex(breakBytes[0],2) + " " + intToHex(breakBytes[1],2));
                    ptr +=2;
                    onByte++;
                    if (breakBytes[0] == 80){ //0x50, catches on rain
                         colorPtr+=3;
                    } else if (breakBytes[0] == 16){ //0x10, catches hail
                         colorPtr+=2;
                    }else{ //else it is 0x30
                        if (breakBytes[1] == 1){ //0x01, catches on normal
                            colorPtr += 3;
                        } else{ //catches on sunny and hail (0x1f and 0x5f)
                            colorPtr += 4;
                        }
                    }
                } else{ //normal
                    if (skippedCol){
//                        System.out.println(" > color skipped");
                        ptr++;
                        skippedCol = false;
                        onByte++;
                    } else if (carryoverByte != -1){
//                        System.out.println(" > carryover");
                        rom[ptr] = intToByte(carryoverByte);
                        ptr++;
                        onByte++;
                        carryoverByte = -1;
                    } else {    //new color
                        if (palStruct.getColor(colorPtr).equals("null")){ //color not used
//                            System.out.println(" > unused color");
                            ptr++;
                            onByte++;
                            skippedCol = true;
                            colorPtr++;
                        } else {
//                            System.out.println(" > new color");
                            rom[ptr] = intToByte(hexToInt(palStruct.getColor(colorPtr).substring(0,2)));
                            carryoverByte = intToByte(hexToInt(palStruct.getColor(colorPtr).substring(2)));
                            ptr++;
                            onByte++;
                            colorPtr++;
                        }
                    }
                }
                focus = focus >> 1;
            }
        }

        if (shiny){
            ptr = addy("d29144");   //E:d29144
            //for (int p=0;p<origPalData.length;p++){ //castform's normal colors are 1 byte longer than its shiny colors
            //The extra byte ran over into its mini-icon, making some pixels show up in its icon.
            //Since the shiny transformational colors are mostly identical, I just stopped before the missing byte (41 in).
            for (int p=0;p<41;p++){
                rom[ptr+p] = intToByte(origPalData[p]);
            }
        }
    }

    private int movePtrToNextPalette(int ptr, int reps){
        if (reps<1) {
            //textSentinel
            System.out.println("ERROR: Cannot move ptr to a negative number!");
            return -1;
        }
        for (int w=0;w<reps;w++){ //pretty badly coded, heh
            while (!(rom[ptr] == 16 && rom[ptr+1] == 32 && rom[ptr+2] == 0 && rom[ptr+3] == 0)){
                ptr++;
            }
            ptr++;
        } ptr--;
//        System.out.println(" > next ptr: " + intToHex(ptr,8));
        return ptr;
    }
    
    private void printDebug(int point){
        StringBuilder out = new StringBuilder("");
        for (int i=0;i<42;i++){
            out.append(intToHex(byteToInt(rom[point+i]),2));
            if (i%4 == 3) out.append(" ");
            if (i%16 == 15) out.append("\n");
        }
        System.out.println(out.toString());
    }

    private void changeTMs(){
        if (!cbTMs.isSelected()) return;
        ArrayList<Integer> list = getAttackList();
        //ArrayList<String> names = getAttackNames();
        String[] attackArray = ak.getAttackListText();
        int ptr = addy("615b94");       //TM data 1
        int ptr2 = addy("616040");      //TM data 2
        int txtptr = addy("582832");    //text in the menus
        int txtptrptr = addy("586b60"); //ptrs to the text in the menus
        int HMstart = addy("583273");   //where the HM text starts
        int tutorPtr = addy("61500c");  //Move Tutor attacks
        int[] TMTextCount = ak.getTMTextFixCount();     //TM NPC text count
        int[] TMTextPtrs = ak.getTMTextFixAddresses();  //Tm NPC text addresses
        String[][] TMText = ak.getTMTextFixes();        //TM NPC text
        int TMTextIndex = 0;                            //pointer to what TM Text we're on
        int chox;

        //load the TM text from the text file
        String[] attackText = new String[354];
        InputStream infi = getClass().getResourceAsStream("resources/TMText.txt");

        try{
            StringBuffer sb = new StringBuffer("");
            char next;
            int i=0;
            while(i<attackText.length && infi.available() > 0){
                next = (char)infi.read();
                if (next == '\r'){
                    infi.read(); //get rid of \n
                    attackText[i] = sb.toString();
                    sb = new StringBuffer("");
                    i++;
                } else{
                    sb.append(next);
                }
            }
            attackText[i] = sb.toString();
        } catch (java.io.IOException ioe){
            JOptionPane.showMessageDialog(null,"An I/O Exception has occurred during TM Text parsing.");
        }

       Random rand = new Random();
        for (int i=0;i<50;i++,ptr+=2,ptr2+=2,txtptrptr+=44){
            //change the TMs
            chox = list.remove(rand.nextInt(list.size()));
            rom[ptr] = intToByte(chox%256);
            rom[ptr+1] = intToByte(chox/256);
            rom[ptr2] = intToByte(chox%256);
            rom[ptr2+1] = intToByte(chox/256);

            //change the menu text
            if (txtptr+attackText[chox].length() > HMstart){
                txtptrptr = addy("e30000");
            }

            //update pointer
            int[] addyParts = getAddyParts(txtptr);
            for (int j=0;j<addyParts.length;j++){
                rom[txtptrptr+j] = intToByte(addyParts[j]);
            }

            //update menu text
            int[] newText = encodeText(attackText[chox-1]);
            for (int p=0;p<newText.length;p++){
                rom[txtptr] = intToByte(newText[p]);
                txtptr++;
            } //terminate with FF
            rom[txtptr] = intToByte(255);
            txtptr++;
//            System.out.println((i)+" - "+names.get(chox));

            //update NPC text
            if (TMTextCount[i] > 0){
                String TM = attackArray[chox];
                int r = rand.nextInt(TMText[TMTextIndex].length);
                for (int j=0;j<TMTextCount[i];j++,TMTextIndex++){
                    StringBuilder sb = new StringBuilder(TMText[TMTextIndex][r]);
                    sb = replaceInText("[1]",TM,sb);
                    writeText(TMTextPtrs[TMTextIndex],sb.toString());
                }
            }
        }

       //Move Tutor moves
       String[][] tutorText = ak.getTutorTextFixes();
       int[] tutorTextPtrs = ak.getTutorTextFixAddresses();
       int[] tutorTextCount = ak.getTutorTextFixCount();
       int tutorTextIndex = 0;
       for (int i=0;i<30;i++,tutorPtr+=2){
            chox = list.remove(rand.nextInt(list.size()));
            rom[tutorPtr] = intToByte(chox%256);
            rom[tutorPtr+1] = intToByte(chox/256);

            //fix NPC text
            if (tutorTextCount[i] > 0){
                String move = attackArray[chox];
                int r = rand.nextInt(tutorText[tutorTextIndex].length);
                for (int j=0;j<tutorTextCount[i];j++,tutorTextIndex++){
                    StringBuilder sb;
                    if (tutorText[tutorTextIndex].length > 1){
                        sb = new StringBuilder(tutorText[tutorTextIndex][r]);
                    } else{
                        sb = new StringBuilder(tutorText[tutorTextIndex][0]);
                    }
                    sb = replaceInText("[1]",move,sb);
                    writeText(tutorTextPtrs[tutorTextIndex],sb.toString());
                }
            }
       }
    }

    private void writeText(int ptr, String text){
        int txtptr = ptr;
        int[] newText = encodeText(text);
        for (int p=0;p<newText.length;p++,txtptr++){
            rom[txtptr] = intToByte(newText[p]);
        }
    }

    private int[] getAddyParts(int ptr){
        int[] out = new int[4];
        for (int i=256,j=0;ptr > 0;j++){
            out[j] = ptr%i;
            ptr = ptr >> 8;
        } out[3] = 8; //addresses always start with 08 (for some reason)
        return out;
    }

    private ArrayList<String> getAttackNames(){
        return decodeTextBlock(addy("319789"),354);
    }

    private String toHex(int i){
        String post,pre;
        if (i/256 > 15) post = java.lang.Integer.toHexString(i/256);
        else post = "0" + java.lang.Integer.toHexString(i/256);
        if (i%256 > 15) pre = java.lang.Integer.toHexString(i%256);
        else pre = "0" + java.lang.Integer.toHexString(i%256);
        return (pre + post).toUpperCase();
    }

    private void openROM(){
        miCloseActionPerformed(null);

        try{
            JFileChooser fc = new JFileChooser();
            ROMFilter rf = new ROMFilter(".GBA Files", "gba");
            fc.setFileFilter(rf);
            int returnVal = fc.showOpenDialog(null);
            if (returnVal==JFileChooser.APPROVE_OPTION){
                File romFile = fc.getSelectedFile();

                //get data from selected save file
                romPath = romFile.getAbsolutePath();
                FileInputStream ist = new FileInputStream(romPath);

                rom = new byte[ist.available()];
                ist.read(rom,0,ist.available());
                ist.close();
                checkHash();

                saved = true;
                setActive(true);
                setStarters();
                unsaved(false);
                setTrainerChoices(false);
                setWildChoices(false);
                setPaletteChoices(false);

                taWild.setText("Choose your randomization options!\n\n"+
                        "If there are any errors using the program, they will appear here.");
            }
        } catch (java.lang.StringIndexOutOfBoundsException jsioobe){
            JOptionPane.showMessageDialog(null,"Error reading file!");
        } catch (NullPointerException npe){
            JOptionPane.showMessageDialog(null,"No filename given!\n"+npe);
        } catch (FileNotFoundException fnfe){
            JOptionPane.showMessageDialog(null,"ROM could not be read!");
        } catch (IOException ioe){
            JOptionPane.showMessageDialog(null,"I/O problem! : "+ioe);
        }
    }

 private boolean miCloseActionPerformed2(){
     if (!saved){
         int ans = JOptionPane.showConfirmDialog(null, "There are still unsaved changes. Save now?");
         if (ans == JOptionPane.CANCEL_OPTION) return false;
         else if (ans == JOptionPane.YES_OPTION){
             saveFile(romPath);
         } else{
             leaveFile(romPath);
         }
     }
     return true;
 }

 private void unsaved(boolean chox){
     saved = !chox;
     miSave.setEnabled(chox);
 }

 private String getDocFileName(){
     String ret = "";
     try{
        JFileChooser fc = new JFileChooser(){
            @Override
            public void approveSelection(){ //from Roberto Luis Bisbé (http://stackoverflow.com/questions/3651494/jfilechooser-with-confirmation-dialog)
                File f = getSelectedFile();
                if(f.exists() && getDialogType() == SAVE_DIALOG){
                    int result = JOptionPane.showConfirmDialog(this,"The file exists. Overwrite?","Existing file",JOptionPane.YES_NO_CANCEL_OPTION);
                    switch(result){
                        case JOptionPane.YES_OPTION:
                            super.approveSelection();
                            return;
                        case JOptionPane.NO_OPTION:
                            return;
                        case JOptionPane.CLOSED_OPTION:
                            return;
                        case JOptionPane.CANCEL_OPTION:
                            cancelSelection();
                            return;
                    }
                }
                super.approveSelection();
            }
        };
        ROMFilter rf = new ROMFilter(".txt File", "txt");
        fc.setFileFilter(rf);
        int returnVal = fc.showSaveDialog(null);
        if (returnVal==JFileChooser.APPROVE_OPTION){
            File romFile = fc.getSelectedFile();
            ret = romFile.getAbsolutePath();
            if (!ret.endsWith(".txt")) ret = ret + ".txt";
        }
     } catch (java.lang.StringIndexOutOfBoundsException jsioobe){
        JOptionPane.showMessageDialog(null,"Error making file!");
     } catch (NullPointerException npe){
        JOptionPane.showMessageDialog(null,"No filename given!\n"+npe);
     }
     return ret;
 }

 private void saveFileAs(){
     try{
        JFileChooser fc = new JFileChooser(){
            @Override
            public void approveSelection(){ //from Roberto Luis Bisbé (http://stackoverflow.com/questions/3651494/jfilechooser-with-confirmation-dialog)
                File f = getSelectedFile();
                if(f.exists() && getDialogType() == SAVE_DIALOG){
                    int result = JOptionPane.showConfirmDialog(this,"The file exists. Overwrite?","Existing file",JOptionPane.YES_NO_CANCEL_OPTION);
                    switch(result){
                        case JOptionPane.YES_OPTION:
                            super.approveSelection();
                            return;
                        case JOptionPane.NO_OPTION:
                            return;
                        case JOptionPane.CLOSED_OPTION:
                            return;
                        case JOptionPane.CANCEL_OPTION:
                            cancelSelection();
                            return;
                    }
                }
                super.approveSelection();
            }
        };
        ROMFilter rf = new ROMFilter(".GBA Files", "gba");
        fc.setFileFilter(rf);
        int returnVal = fc.showSaveDialog(null);
        if (returnVal==JFileChooser.APPROVE_OPTION){
            File romFile = fc.getSelectedFile();
            romPath = romFile.getAbsolutePath();
            if (!romPath.toLowerCase().endsWith(".gba")) romPath = romPath + ".gba";
            saveFile(romPath);
        }
     } catch (java.lang.StringIndexOutOfBoundsException jsioobe){
        JOptionPane.showMessageDialog(null,"Error reading file!");
     } catch (NullPointerException npe){
        JOptionPane.showMessageDialog(null,"No filename given!\n"+npe);
     }
 }

 private int saveFile(String romPath){
     try {
         FileOutputStream os = createOutputStream(romPath);
         changeROM();
         os.write(rom);
         os.close();
         saved = true;
         miSave.setEnabled(false);
         JOptionPane.showMessageDialog(null, "File saved successfully!");
         return 0;
     } catch (IOException ioe) {
         JOptionPane.showMessageDialog(null, "There was an error while saving:\n" + ioe);
         return 1;
     }
 }

 private int leaveFile(String romPath){
     try {
         FileOutputStream os = createOutputStream(romPath);
         os.write(rom);
         os.close();
         saved = true;
         miSave.setEnabled(false);
         return 0;
     } catch (IOException ioe) {
         JOptionPane.showMessageDialog(null, "There was an error while closing:\n" + ioe);
         return 1;
     }
 }

 private void setStarters(){
     // 5971448, 5971450, 5971452 for yours
     int[] index = {0,0,0};
     int[] trans = ak.getPkmnGameToDexTranscription();
     String[] pkmnList = ak.getPokemonMenuList();
     for (int place = addy("5b1df8"); place < addy("5b1dfd"); place +=2){
         int num = byteToInt(rom[place]) + 256 * byteToInt(rom[place+1]);
         num = trans[num-1];
         if (num>pkmnList.length-1) num = 0; //-1 because of random
         index[(place-addy("5b1df8"))/2] = num;
     }
     cbStarter1.setSelectedIndex(index[0]);
     cbStarter2.setSelectedIndex(index[1]);
     cbStarter3.setSelectedIndex(index[2]);
     //Item at 725370
     int item = byteToInt(rom[725370]) + byteToInt(rom[725372]);
     if (item>(new ArrayKeeper().getItemList()).length) item = 0;
     cbItem.setSelectedIndex(item);
 }

 private void changeStarters(){
     Random rand = new Random();
     String[] pkmnNameList = ak.getPokemonListGameOrder();
     int[] trans = ak.getPkmnDexToGameTranscription();
     ArrayList<Integer> pkmnList;
     //set starters-------------------------------
     int[] starters = {cbStarter1.getSelectedIndex(),cbStarter2.getSelectedIndex(),
                       cbStarter3.getSelectedIndex()};

     //basic pokemon only
     if (cbEnsureBasic.isSelected()){
         pkmnList = ak.getArrayListInt(ak.getBasicPokemonGame());
     } else{
         pkmnList = ak.getArrayListInt(ak.getPokemonListGame());
     }

     //no legendary starters
     if (cbNoLegends.isSelected()){
         pkmnList.removeAll(ak.getArrayListInt(ak.getLegendaryArrayGame()));
     }

     for (int i=0; i<3; i++){
         if (starters[i] == 0) {
            starters[i] = pkmnList.remove(rand.nextInt(pkmnList.size()));
         } else{
             starters[i] = trans[starters[i]-1];
         }
     } //now, actually set them in rom
     int place = addy("5b1df8");//5971448;
     for (int i=0; i<3; i++, place+=2){
         rom[place] = intToByte(starters[i] % 256);
         rom[place+1] = intToByte(starters[i] / 256);
     }

     //set item---------------------------------
     place = addy("b117a");//725370;
     int item = cbItem.getSelectedIndex();
     rom[place] = intToByte(Math.min(item, 255));
     rom[place+2] = intToByte(Math.max(0,item-255));
     rom[place+3] = intToByte(50);


     //change rival's first pokemon
     changeRivalStarters(starters);

     //fix NPC text
     if (!normalStarters()){
         String[] treecko = new String[]{"[1]#is a SWEET type.$It's strong against SOUR and#BITTER types.$But, it's weak against"+
                                            " LAME-type#POK@MON.*",
                                         "#[1]is a FLUFF type.$It's strong against BALD and#UGLY types.$But, it's weak against "+
                                            "COMB-type#POK@MON.*",
                                         "[1]#is a WEIRD type.$It's strong against BOLD and#VANILLA types.$But, it's weak against "+
                                            "LOGIC-type#POK@MON.*"};
         String[] torchic = new String[]{"[1]#is a LAZY type.$It's strong against VIM and#VIGOR types.$But, it's weak against SLEEP-type#POK@MON.*",
                                         "[1]#is a BIRD type.$It's strong against GLITCH and#BUG types.$But, it's weak against PATCH-type#POK@MON.*",
                                         "[1]#is a PLOT type.$It's strong against the#PLAYER type.$But, it's weak against SPOILER-type#POK@MON.*"};
         String[] mudkip  = new String[]{"[1]#is a PAY type.$It's strong against BANK types.$But, it's weak against CRIMINAL-"+
                                            "type#and PIRATE-type POK@MON.*",
                                         "[1]#is a COOKIE type.$It's strong against DIET type.$But, it's weak against MILK-type#and STOMACH-"+
                                            "type POK@MON.*",
                                         "[1]#is a QUIET type.$It's strong against LIBRARY type.$But, it's weak against LOUD-type#"+
                                            "and MUSIC-type POK@MON.*"};

         StringBuilder sb = new StringBuilder(treecko[rand.nextInt(treecko.length)]);
         replaceInText("[1]",pkmnNameList[starters[0]-1],sb);
         writeText(addy("207bc2"),sb.toString());
         sb = new StringBuilder(torchic[rand.nextInt(torchic.length)]);
         replaceInText("[1]",pkmnNameList[starters[1]-1],sb);
         writeText(addy("207c47"),sb.toString());
         sb = new StringBuilder(mudkip[rand.nextInt(mudkip.length)]);
         replaceInText("[1]",pkmnNameList[starters[2]-1],sb);
         writeText(addy("207cc9"),sb.toString());
     }
 }

 private boolean normalStarters(){
     return (cbStarter1.getSelectedIndex() == 252) &&
             (cbStarter2.getSelectedIndex() == 255) &&
             (cbStarter3.getSelectedIndex() == 258);
 }

 private void changeRivalStarters(int[] starters){
     //Treecko
     rom[addy("30db30")] = intToByte(starters[0] % 256);
     rom[addy("30db31")] = intToByte(starters[0] / 256);
     rom[addy("30dbd8")] = intToByte(starters[0] % 256);
     rom[addy("30dbd9")] = intToByte(starters[0] / 256);

     //Torchic
     rom[addy("30db68")] = intToByte(starters[1] % 256);
     rom[addy("30db69")] = intToByte(starters[1] / 256);
     rom[addy("30dc10")] = intToByte(starters[1] % 256);
     rom[addy("30dc11")] = intToByte(starters[1] / 256);

     //Mudkip
     rom[addy("30dba0")] = intToByte(starters[2] % 256);
     rom[addy("30dba1")] = intToByte(starters[2] / 256);
     rom[addy("30dc48")] = intToByte(starters[2] % 256);
     rom[addy("30dc49")] = intToByte(starters[2] / 256);
 }

 private void changeWildPokemon(){
     if (rbUnchanged.isSelected()){
         if (cbRandTrades.isSelected()){
             randomizeTrades();
         }
         if (cbUnique.isSelected()){
             randomizeUniques();
         }
         return;
     }
     Random rand = new Random();
     //random pokemon============================================
     int[] addresses = ak.getEncounterSlotAddresses();
     int[] lengths = ak.getEncounterSlotLengths();

     List<Integer> trades = new ArrayList<Integer>();
     List<Integer> uniques = new ArrayList<Integer>();

     if (!rbGlobal.isSelected()){ //global has its own replacement methods for these
         if (cbRandTrades.isSelected()){
             trades = randomizeTrades();
         }
         if (cbUnique.isSelected()){
             uniques = randomizeUniques();
         }
     }
     
     List<Integer> pkmnList = generatePokemonArray(trades, uniques);
     List<Integer> safari = generatePokemonArray(trades, uniques);
     int[][] family = ak.getFamilyTree();
     boolean ensure = cbEnsureAll.isSelected();
     boolean form = cbKeepForm.isSelected();
     int[] specialSlots = ak.getSpecialEncounterSlots();
     int poke = 1;
     int oldPoke = 0;
     int at = 0;

     if (rbRandom.isSelected()){//-----random----------------------------------------------------
         for (int i=0;i<addresses.length;i++){
             int[] lenParts = getLenParts(lengths[i]);
             if ( indexOf(specialSlots,i) != -1){ //make sure this isn't the only spot to find pkmn
                 int added = 0;
                 for (int k=0; k<lenParts.length; k++){
                     for (int j=0;j<lenParts[k];j++){
                         at = addresses[i]+(4*j)+added;
                         poke = safari.get(rand.nextInt(safari.size()));
                         if (form){
                             oldPoke = byteToInt(rom[at]) + byteToInt(rom[at+1])*256;
                             if (indexOf(family[oldPoke-1],oldPoke) != 0) { //the original's not basic
                                 if (indexOf(family[poke-1],poke) == 0) { //the replacement is basic
                                     if (family[poke-1].length > 1){
                                         poke = family[poke-1][1]; //nab first evo
                                     } //else it doesn't evolve
                                 } //else the replace isn't basic, too, so do nothing
                             } else{ //the original's basic
                                 if (indexOf(family[poke-1],poke) != 0) { //the replacement isn't basic
                                     poke = family[poke-1][0];
                                 } //else the replace is basic, too, so do nothing
                             }
                         } //else whatever was picked is fine
                         rom[at] = intToByte(poke%256);
                         rom[at+1] = intToByte(poke/256);
                     }
                     added += lenParts[k]*4 + 8;
                 }
             } else {
                 int added = 0;
                 for (int k=0; k<lenParts.length; k++){
                     for (int j=0;j<lenParts[k];j++){
                         at = addresses[i]+(4*j)+added;
                         if (pkmnList.size() == 0){ pkmnList = generatePokemonArray(trades, uniques); }

                         if (ensure){
                             poke = pkmnList.remove(rand.nextInt(pkmnList.size()));
                         } else{
                             poke = pkmnList.get(rand.nextInt(pkmnList.size()));
                         }

                         if (form){
                             oldPoke = byteToInt(rom[at]) + byteToInt(rom[at+1])*256;
                             if (indexOf(family[oldPoke-1],oldPoke) != 0) { //the original's not basic
                                 if (indexOf(family[poke-1],poke) == 0) { //the replacement is basic
                                     if (family[poke-1].length > 1){
                                         poke = family[poke-1][1]; //nab first evo
                                     } //else it doesn't evolve
                                 } //else the replace isn't basic, too, so do nothing
                             } else{ //the original's basic
                                 if (indexOf(family[poke-1],poke) != 0) { //the replacement isn't basic
                                     poke = family[poke-1][0];
                                 } //else the replace is basic, too, so do nothing
                             }
                         } //else whatever was picked is fine
                         rom[at] = intToByte(poke%256);
                         rom[at+1] = intToByte(poke/256);
                     }
                     added += lenParts[k]*4 + 8;
                 }
             }
         }
     } else if(rbSubs.isSelected()){ //Single substitution================================
         for (int i=0;i<addresses.length;i++){
             int[] lenParts = getLenParts(lengths[i]);
             if ( indexOf(specialSlots,i) != -1){ //make sure this isn't the only spot to find pkmn
                 int added = 0;
                 for (int k=0; k<lenParts.length; k++){
                     for (int j=0;j<lenParts[k];j++){
                         at = addresses[i]+(4*j)+added;
                         HashMap<Integer,Integer> hm = new HashMap<Integer,Integer>();
                         oldPoke = byteToInt(rom[at]) + 256*byteToInt(rom[at+1]);
                         if (!hm.containsKey(oldPoke)){
                             poke = safari.get(rand.nextInt(safari.size()));
                             if (form){
                                 if (indexOf(family[oldPoke-1],oldPoke) != 0) { //the original's not basic
                                     if (indexOf(family[poke-1],poke) == 0) { //the replacement is basic
                                         if (family[poke-1].length > 1){
                                             poke = family[poke-1][1]; //nab first evo
                                         } //else it doesn't evolve
                                     } //else the replace isn't basic, too, so do nothing
                                 } else{ //the original's basic
                                     if (indexOf(family[poke-1],poke) != 0) { //the replacement isn't basic
                                         poke = family[poke-1][0];
                                     } //else the replace is basic, too, so do nothing
                                 }
                             } //else whatever was picked is fine
                             hm.put(oldPoke, poke);
                         }
                         poke = hm.get(oldPoke);
                         rom[at] = intToByte(poke%256);
                         rom[at+1] = intToByte(poke/256);
                     } added += lenParts[k]*4 + 8;
                 }
             } else {
                 int added = 0;
                 for (int k=0; k<lenParts.length; k++){
                     HashMap<Integer,Integer> hm = new HashMap<Integer,Integer>();
                     for (int j=0;j<lenParts[k];j++){
                         at = addresses[i]+(4*j)+added;
                         if (pkmnList.size() == 0){ pkmnList = generatePokemonArray(trades, uniques); }
                         oldPoke = byteToInt(rom[at]) + 256*byteToInt(rom[at+1]);

                         if (!hm.containsKey(oldPoke)){
                             if (ensure){
                                 poke = pkmnList.remove(rand.nextInt(pkmnList.size()));
                             } else{
                                 poke = pkmnList.get(rand.nextInt(pkmnList.size()));
                             }

                             if (form){
                                 if (indexOf(family[oldPoke-1],oldPoke) != 0) { //the original's not basic
                                     if (indexOf(family[poke-1],poke) == 0) { //the replacement is basic
                                         if (family[poke-1].length > 1){
                                             poke = family[poke-1][1]; //nab first evo
                                         } //else it doesn't evolve
                                     } //else the replace isn't basic, too, so do nothing
                                 } else{ //the original's basic
                                     if (indexOf(family[poke-1],poke) != 0) { //the replacement isn't basic
                                         poke = family[poke-1][0];
                                     } //else the replace is basic, too, so do nothing
                                 }
                             } //else whatever was picked is fine

                             hm.put(oldPoke, poke);
                         } else{
                             poke = hm.get(oldPoke);
                         }
                         
                         rom[at] = intToByte(poke%256);
                         rom[at+1] = intToByte(poke/256);
                     }
                     added += lenParts[k]*4 + 8;
                 }
             }
         }
     } else if(rbGlobal.isSelected()){ //Global substitution===============================
         HashMap<Integer,Integer> replace = new HashMap<Integer,Integer>();
         int replacements = 0;
         for (int i=0;i<addresses.length;i++){
             int[] lenParts = getLenParts(lengths[i]);
             if ( indexOf(specialSlots,i) != -1){ //make sure this isn't the only spot to find pkmn
                 int added = 0;
                 for (int k=0; k<lenParts.length; k++){
                     for (int j=0;j<lenParts[k];j++){
                         at = addresses[i]+(4*j)+added;

                         oldPoke = byteToInt(rom[at]) + 256*byteToInt(rom[at+1]);
                         if (!replace.containsKey(oldPoke)){
                             poke = safari.get(rand.nextInt(safari.size()));
                             if (form){
                                 if (indexOf(family[oldPoke-1],oldPoke) != 0) { //the original's not basic
                                     if (indexOf(family[poke-1],poke) == 0) { //the replacement is basic
                                         if (family[poke-1].length > 1){
                                             poke = family[poke-1][1]; //nab first evo
                                         } //else it doesn't evolve
                                     } //else the replace isn't basic, too, so do nothing
                                 } else{ //the original's basic
                                     if (indexOf(family[poke-1],poke) != 0) { //the replacement isn't basic
                                         poke = family[poke-1][0];
                                     } //else the replace is basic, too, so do nothing
                                 }
                             } //else whatever was picked is fine
                             replace.put(oldPoke, poke);
                             replacements++;
                         }
                         poke = replace.get(oldPoke);
                         rom[at] = intToByte(poke%256);
                         rom[at+1] = intToByte(poke/256);
                     } added += lenParts[k]*4 + 8;
                 }
             } else {
                 int added = 0;
                 for (int k=0; k<lenParts.length; k++){
                     for (int j=0;j<lenParts[k];j++){
                         at = addresses[i]+(4*j)+added;
                         if (pkmnList.size() == 0){ pkmnList = generatePokemonArray(trades, uniques); }
                         oldPoke = byteToInt(rom[at]) + 256*byteToInt(rom[at+1]);

                         if (!replace.containsKey(oldPoke)){
                             if (ensure){
                                 poke = pkmnList.remove(rand.nextInt(pkmnList.size()));
                                 pkmnList.removeAll(ak.getArrayListInt(family[poke-1]));
                             } else{
                                 poke = pkmnList.get(rand.nextInt(pkmnList.size()));
                             }

                             if (form){
                                 if (indexOf(family[oldPoke-1],oldPoke) != 0) { //the original's not basic
                                     if (indexOf(family[poke-1],poke) == 0) { //the replacement is basic
                                         if (family[poke-1].length > 1){
                                             poke = family[poke-1][1]; //nab first evo
                                         } //else it doesn't evolve
                                     } //else the replace isn't basic, too, so do nothing
                                 } else{ //the original's basic
                                     if (indexOf(family[poke-1],poke) != 0) { //the replacement isn't basic
                                         poke = family[poke-1][0];
                                     } //else the replace is basic, too, so do nothing
                                 }
                             } //else whatever was picked is fine

                             replace.put(oldPoke, poke);
                             replacements++;
                         } else{
                             poke = replace.get(oldPoke);
                         }

                         rom[at] = intToByte(poke%256);
                         rom[at+1] = intToByte(poke/256);
                     }
                     added += lenParts[k]*4 + 8;
                 }
             }
         }
//         System.out.println(" > Replacements: " + replacements + " -- basic pkmn: " + ak.getBasicPokemonGame().length);

         //fill the rest of replace
         for (int entry : ak.getPokemonListGame()){
             if (!replace.containsKey(entry)){
                 if (pkmnList.size() == 0){ pkmnList = generatePokemonArray(trades, uniques); }
                 replace.put(entry, pkmnList.remove(rand.nextInt(pkmnList.size())));
             }
         }

         //trades
         if (cbRandTrades.isSelected()){
             int[] tradeOrig = new int[]{
                    replace.get(byteToInt(rom[addy("338edc")]) + 256*byteToInt(rom[addy("338edd")])),
                    replace.get(byteToInt(rom[addy("338f18")]) + 256*byteToInt(rom[addy("338f19")])),
                    replace.get(byteToInt(rom[addy("338f54")]) + 256*byteToInt(rom[addy("338f55")])),
                    replace.get(byteToInt(rom[addy("338f90")]) + 256*byteToInt(rom[addy("338f91")])),

                    replace.get(byteToInt(rom[addy("338f08")]) + 256*byteToInt(rom[addy("338f09")])),
                    replace.get(byteToInt(rom[addy("338f44")]) + 256*byteToInt(rom[addy("338f45")])),
                    replace.get(byteToInt(rom[addy("338f80")]) + 256*byteToInt(rom[addy("338f81")])),
                    replace.get(byteToInt(rom[addy("338fbc")]) + 256*byteToInt(rom[addy("338fbd")])),
             };
             randomizeTrades(tradeOrig);
         }

         //uniques
         randomizeUniques(replace);
     }
     else {
         //textSentinel
        System.out.println("ERROR: Not a valid wild pokemon change!");
     }
     //testEncounters();

     //replace pokemon that attacks Prof. Birch
     int birchPtr = addy("32706");
     int[] pkmnList2 = ak.getPokemonListGame();
     poke = pkmnList2[rand.nextInt(pkmnList2.length)];

     if (poke <= 255){
         writeToROM(birchPtr,new int[]{poke,33,0,0}); // ##210000
     } else if(poke > 255+256){
         JOptionPane.showMessageDialog(null, "Error setting Birch's attacking pokemon.\n"
                 + "No change was made.");
     } else{
         writeToROM(birchPtr,new int[]{poke-255,33,255,49}); //##21FF31
     }
 }

 private void testEncounters(){
     int[] addresses = ak.getEncounterSlotAddresses();
     int[] lengths = ak.getEncounterSlotLengths();
     int at,poke;
     ArrayList<Integer> used = ak.getArrayListInt(ak.getPokemonListGame());
     int[][] family = ak.getFamilyTree();

     for (int i=0;i<addresses.length;i++){
            int[] lenParts = getLenParts(lengths[i]);
            int added = 0;
            for (int k=0; k<lenParts.length; k++){
                     for (int j=0;j<lenParts[k];j++){
                         at = addresses[i]+(4*j)+added;
                         poke = byteToInt(rom[at]) + byteToInt(rom[at+1])*256;
                         if (used.contains(poke)){

                             for (int mem : family[poke-1]){
                                 used.remove(new Integer(mem));
                             }
                         }
                     }
                     added += lenParts[k]*4 + 8;
                 }
             }

//     System.out.println(" >> Pokemon left: " + used.size());
 }

 private ArrayList<Integer> randomizeUniques(){
     int[][] addys = ak.getUniquePokemonAddys();
     int[][] johtoAddys = ak.getJohtoStarterAddys();

     // 25 choices; 21 legendaries. 5 choices are GS only. Make Lati@s both available at dex completion

     int[] choices = new int[addys.length];
     int[] johto = new int[3];
     Random rand = new Random();
     ArrayList<Integer> out = new ArrayList<Integer>();

     if (cbUniqueLegends.isSelected()){
         ArrayList<Integer> alPkmn = ak.getArrayListInt(ak.getLegendaryArrayGame());

         //Lati@s
         choices[16] = alPkmn.remove(rand.nextInt(11)); //these need to be non-hoenn so the player
         choices[17] = alPkmn.remove(rand.nextInt(10)); //can complete the dex to get all 386 naturally

         //the rest
         for (int i=0;i<choices.length;i++){
             choices[i] = alPkmn.remove(rand.nextInt(alPkmn.size()));
             if (i==15) i+=2;   //Lati@s
             if (i==18) i+=4;   //Mew, Deoxys, Lugia, Ho-oh
         }

         //replace Gameshark-only pokemon (Mew, Deoxys, Lugia, Ho-oh)
         alPkmn = ak.getArrayListInt(ak.getLegendaryArrayGame()); //replenish; all 21 are used up by now
         for (int i=19;i<23;i++){
             choices[i] = alPkmn.remove(rand.nextInt(alPkmn.size()));
         }

         //Johto starters
         johto[0] = choices[16]; //same as Lati@s so player can get both
         johto[1] = choices[17];
         int last;
         do{
             last = alPkmn.remove(rand.nextInt(alPkmn.size()));
         } while (last == choices[16] || last == choices[17]);
         johto[2] = last;

         //output
         out = ak.getArrayListInt(choices);
     } else{
         ArrayList<Integer> alPkmn = ak.getArrayListInt(ak.getPokemonListGame());
         int[][] family = ak.getFamilyTree();

         int pkmn = 0;
         for (int i=0;i<choices.length;i++){
             pkmn = alPkmn.remove(rand.nextInt(alPkmn.size()));
             choices[i] = family[pkmn-1][family[pkmn-1].length-1];  //last evolution
             ArrayList<Integer> fam = ak.getArrayListInt(family[choices[i]-1]);
             alPkmn.removeAll(fam); //prevent multiple legends in same family line

             //output
             out.addAll(fam);
         }

         //Johto starters
         johto[0] = choices[16];
         johto[1] = choices[17];
         johto[2] = alPkmn.remove(rand.nextInt(alPkmn.size())); //not counted as unique location
     }

     //roamable Latios
     int ptr = addy("161bc4");
     rom[ptr] = intToByte(choices[16] % 255);
     rom[ptr+2] = intToByte(Math.min(255 * (choices[16]/255),255));
     rom[ptr+3] = intToByte(50);    //0x32 -- replaces the bit shift with an add

     //replace uniques
     for (int i=0;i<addys.length;i++){
         for (int j=0;j<addys[i].length;j++){
             rom[addys[i][j]] = intToByte(choices[i] % 256);
             rom[addys[i][j] + 1] = intToByte(choices[i] / 256);
         }
     }

     //replace Johto starters
     for (int i=0;i<johtoAddys.length;i++){
         for (int j=0;j<johtoAddys[i].length;j++){
             rom[johtoAddys[i][j]] = intToByte(johto[i] % 256);
             rom[johtoAddys[i][j] + 1] = intToByte(johto[i] / 256);
         }
     }

     fixRoamablePkmnText(choices,johto);
     return out;
 }

 private void randomizeUniques(HashMap<Integer, Integer> replace){
     int[][] addys = ak.getUniquePokemonAddys();
     int[][] johtoAddys = ak.getJohtoStarterAddys();
     int pkmn=0;

     //replace uniques
     for (int i=0;i<addys.length;i++){
         pkmn = replace.get(byteToInt(rom[addys[i][0]]) + byteToInt(rom[addys[i][0]+1]) * 256);
         for (int j=0;j<addys[i].length;j++){
             rom[addys[i][j]] = intToByte(pkmn % 256);
             rom[addys[i][j] + 1] = intToByte(pkmn / 256);
         }
     }

     //replace Johto starters
     for (int i=0;i<johtoAddys.length;i++){
         pkmn = replace.get(byteToInt(rom[johtoAddys[i][0]]) + byteToInt(rom[johtoAddys[i][0]+1]) * 256);
         for (int j=0;j<johtoAddys[i].length;j++){
             rom[johtoAddys[i][j]] = intToByte(pkmn % 256);
             rom[johtoAddys[i][j] + 1] = intToByte(pkmn / 256);
         }
     }
 }

 private List<Integer> randomizeTrades(){
     List<Integer> pkmn = ak.getArrayListInt(ak.getPokemonListGame());
     List<Integer> items = ak.getArrayListInt(ak.getUsableItems());
     List<String> names = ak.getArrayListString(ak.getTradePokemonNames());
     Random rand = new Random();
     List<Integer> out = new ArrayList<Integer>();

     //Ralts, Volbeat, Bagon, Skitty
     int ptr = addy("338ed0");
     int[] textPtrs = new int[]{0,addy("2164ed"),addy("203eb7"),addy("265218")};
     String[] text = new String[]{"",
                "[2] super#strong right away!$I hope you do the same#with [1]!*",
                "wanted to get a [2]#that another TRAINER caught~*",
                "[2] is so much cuter than I had^imagined. I love it!*"};
     int temp=0;
     int given=0;
     String[] pkmnList = ak.getPokemonListGameOrder();
     for (int i=0;i<4;i++,ptr+=60){
         //name
         int[] name = encodeText(names.remove(rand.nextInt(names.size())));
         int n=0;
         for (;n<name.length;n++){
             rom[ptr+n] = intToByte(name[n]);
         }
         rom[ptr+n] = intToByte(255);   //terminal FF
         for (int m=n+1;m<11;m++){      //fill the rest of the slots with 0
             rom[ptr+m] = intToByte(0);
         }

         //pokemon given by NPC
         given = pkmn.remove(rand.nextInt(pkmn.size()));
         rom[ptr+12] = intToByte(given % 256);
         rom[ptr+13] = intToByte(given / 256);
         out.add(given);

         //IVs
         for (int p=0;p<6;p++){
             rom[ptr+14+p] = intToByte(rand.nextInt(32));
         }

         //item
         temp = items.remove(rand.nextInt(items.size()));
         rom[ptr+40] = intToByte(temp % 256);
         rom[ptr+41] = intToByte(temp / 256);

         //pokemon given by trainer
         temp = pkmn.remove(rand.nextInt(pkmn.size()));
         rom[ptr+56] = intToByte(temp % 256);
         rom[ptr+57] = intToByte(temp / 256);

         //text
         if (textPtrs[i] > 0){
             StringBuilder sb = new StringBuilder(text[i]);
             sb = replaceInText("[1]",pkmnList[given-1],sb);
             sb = replaceInText("[2]",pkmnList[temp-1],sb);
             writeText(textPtrs[i],sb.toString());
         }
     }
     return out;
 }

 private StringBuilder replaceInText(String seq, String replacement, StringBuilder sb){
     int ind = sb.indexOf(seq);
     while (ind >= 0){
         sb.replace(ind, ind+seq.length(), replacement);
         ind = sb.indexOf(seq);
     }
     return sb;
 }

 private void randomizeTrades(int[] pkin){
     List<Integer> items = ak.getArrayListInt(ak.getUsableItems());
     List<String> names = ak.getArrayListString(ak.getTradePokemonNames());
     Random rand = new Random();

     //Ralts, Volbeat, Bagon, Skitty
     int ptr = addy("338ed0");
     int[] textPtrs = new int[]{0,addy("2164ed"),addy("203eb7"),addy("265218")};
     String[] text = new String[]{"",
                "[2] super#strong right away!  $I hope you do the same#with [1]!",
                "wanted to get a [2]#that another TRAINER caught~  ",
                "[2] is so much cuter than I had^imagined. I love it!"};
     int temp=0;
     String[] pkmnList = ak.getPokemonListGameOrder();
     for (int i=0;i<4;i++,ptr+=60){
         //name
         int[] name = encodeText(names.remove(rand.nextInt(names.size())));
         int n=0;
         for (;n<name.length;n++){
             rom[ptr+n] = intToByte(name[n]);
         }
         rom[ptr+n] = intToByte(255);   //terminal FF
         for (int m=n+1;m<11;m++){      //fill the rest of the slots with 0
             rom[ptr+m] = intToByte(0);
         }

         //pokemon given by NPC
         rom[ptr+12] = intToByte(pkin[i] % 256);
         rom[ptr+13] = intToByte(pkin[i] / 256);

         //IVs
         for (int p=0;p<6;p++){
             rom[ptr+14+p] = intToByte(rand.nextInt(32));
         }

         //item
         temp = items.remove(rand.nextInt(items.size()));
         rom[ptr+40] = intToByte(temp % 256);
         rom[ptr+41] = intToByte(temp / 256);

         //pokemon given by trainer
         rom[ptr+56] = intToByte(pkin[i+4] % 256);
         rom[ptr+57] = intToByte(pkin[i+4] / 256);

         //text
         if (textPtrs[i] > 0){
             StringBuilder sb = new StringBuilder(text[i]);
             sb = replaceInText("[1]",pkmnList[pkin[i]-1],sb);
             sb = replaceInText("[2]",pkmnList[pkin[i+4]-1],sb);
             writeText(textPtrs[i],sb.toString());
         }
     }
 }

 private ArrayList<Integer> generatePokemonArray(List<Integer> trades, List<Integer> uniques){
     ArrayList<Integer> pkmn = ak.getArrayListInt(ak.getPokemonListGame());
     List<Integer> legends = ak.getArrayListInt(ak.getLegendaryArrayGame());
     if (cbNoLegends.isSelected()){
         pkmn.removeAll(legends);
     }
     pkmn.removeAll(trades);
     pkmn.removeAll(uniques);
     return pkmn;
 }

 private ArrayList<Integer> generatePokemonList(boolean noLegends){
     ArrayList<Integer> pkmn = ak.getArrayListInt(ak.getPokemonListGame());
     if (noLegends){
         pkmn.removeAll(ak.getArrayListInt(ak.getLegendaryArrayGame()));
     }
     return pkmn;
 }

 private ArrayList<Integer> generatePokemonArray(){
     return ak.getArrayListInt(ak.getPokemonListGame());
 }

 private void fixDisobedience(){ //thanks Dabomstew!
     rom[addy("45c76")] = 0;
     rom[addy("45c77")] = 0;
     rom[addy("45c88")] = 0;
     rom[addy("45c89")] = 0;
 }

 private int[] getLenParts(int len){
     int[] lenParts;
     switch (len){
         case 5:
         case 10:
         case 12: //grass
             lenParts = new int[1];
             lenParts[0] = len;
             break;
         case 15: //water, fish
             lenParts = new int[2];
             lenParts[0] = 5; lenParts[1] = 10;
             break;
         case 17: //grass, rock
             lenParts = new int[2];
             lenParts[0] = 12; lenParts[1] = 5;
             break;
         case 27: //grass, water, fish
             lenParts = new int[3];
             lenParts[0] = 12; lenParts[1] = 5; lenParts[2] = 10;
             break;
         case 32: //grass, water, rock, fish
             lenParts = new int[4];
             lenParts[0] = 12; lenParts[1] = 5; lenParts[2] = 5; lenParts[3] = 10;
             break;
         default:
             lenParts = new int[0];
             //textSentinel
             System.out.println("ERROR: some length is wrong!");
     } return lenParts;
 }

 //convert a byte to an int
 private int byteToInt(byte b){
     return (int) b & 0xFF;
 }

 //convert an int to hex
 private byte intToByte(int i){
     if (i>127){
         return (byte)(i-256);
     } else return (byte)i;
 }

 private int hexToInt(String str){
    return ((int)((byte)Integer.valueOf(str, 16).intValue()) & 0xFF);
 }

 private String intToHex(int n, int size){
     StringBuffer out = new StringBuffer(java.lang.Integer.toHexString(n));
     for (int i=size-out.length();i>0;i--){
         out.insert(0,"0");
     }
     if (out.length() > size){
         //textSentinel
         System.out.println("intToHex error: int " + n + " too big for size " + size);
         return out.toString().substring(out.length()-size);
     }
     return out.toString();
 }

 private void writeToROM(int[] ar, int ptr){
     for (int i=0;i<ar.length;i++){
         rom[ptr+i] = intToByte(ar[i]);
     }
 }

 private ArrayList<Integer> getAttackList(){
     ArrayList<Integer> list = new ArrayList<Integer>();
     for (int i=0;i<=353;i++){ // a5 is Struggle and end; 00 = nothing
         list.add(i);
     }
     //These must be in order from highest to lowest!
     list.remove(256+hexToInt("23")); //Dive
     list.remove(256+hexToInt("00")); //Swallow
     list.remove(hexToInt("ff")); //Spit Up
     list.remove(hexToInt("fe")); //Stockpile
     list.remove(hexToInt("f9")); //Rock smash
     list.remove(hexToInt("a5")); //Struggle
     list.remove(hexToInt("94")); //Flash
     list.remove(hexToInt("7f")); //Waterfall
     list.remove(hexToInt("46")); //Strength
     list.remove(hexToInt("39")); //Surf
     list.remove(hexToInt("13")); //Fly
     list.remove(hexToInt("0f")); //Cut
     list.remove(hexToInt("00")); //Nothing
    
     return list;
 }

 //create a file to output from a string filename
 private FileOutputStream createOutputStream(String name){
     try{
         return new FileOutputStream(new File(name));
     } catch (IOException ioe) {
         JOptionPane.showMessageDialog(null, "Error creating the output stream:\n" + ioe);
         return null;
     }
 }

 private ArrayList<String> decodeTextBlock(int addy, int len){
     Integer[] symbnum = ak.getSymbolTranscriptionList();
     Character[] symb = ak.getSymbolList();

     ArrayList<String> out = new ArrayList<String>();
     int ptr = addy;
     int next;
     String nom;
     for (int i=0;i<len;i++){
         nom = "";
         for (int j=0;j<13;j++,ptr++){ //each block is 12 long, with space for a FF terminator
             next = (rom[ptr]+256)%256;
             System.out.println(next);
             if (next == 255){
                 ptr+=13-j;
                 break;
             }
             if (next >= 187 && next <= 212){         //Upper
                nom=nom.concat((char)(next - 122)+"");
             } else if (next >=213 && next <= 238){  //Lower
                nom=nom.concat((char)(next - 116)+"");
             } else if (next >=161 && next <= 170){   //Number
                nom=nom.concat((char)(next - 113)+"");
             } else if (indexOf(symbnum,next) != -1){
                nom=nom.concat(symb[indexOf(symbnum,next)]+"");
             } else{
                 //textSentinel
                System.out.println("Unknown char dropped: "+next);
                nom=nom.concat("*");
             }
         }
         out.add(nom);
     }
     return out;
 }

 private String decodeText(int addy, int len){
     Integer[] symbnum = ak.getSymbolTranscriptionList();
     Character[] symb = ak.getSymbolList();

     ArrayList<String> out = new ArrayList<String>();
     int ptr = addy;
     int next;
     StringBuilder sb = new StringBuilder("");
     for (int i=0;i<len;i++,ptr++){
         next = (rom[ptr]+256)%256;
         if (next >= 187 && next <= 212){         //Upper
            sb.append((char)(next - 122)+"");
         } else if (next >=213 && next <= 238){  //Lower
            sb.append((char)(next - 116)+"");
         } else if (next >=161 && next <= 170){   //Number
            sb.append((char)(next - 113)+"");
         } else if (indexOf(symbnum,next) != -1){
            sb.append(symb[indexOf(symbnum,next)]+"");
         } else{
             //textSentinel
            System.out.println("Unknown char dropped: "+next);
            sb.append("*");
         }
     }
     return sb.toString();
 }

  private int[] encodeText(String input){
     Character[] symb = {' ','$', ':', '?', '!', '.', '`', '\'','-', '#', '@', ',', '♀', '♂', '<', '>', '(', ')', '/', '~', '^', '*'};
     String[] symbnum = {"00","fb","f0","ac","ab","ad","b3","b4","ae","fe","1b","b8","b6","b5","53","54","5c","5d","ba","b0","fa","ff"};
     int[] out = new int[input.length()];
     int remove = 0;
     for (int i=0;i<input.length();i++){
         int part = (int)(input.charAt(i));
         if (part >= 65 && part <= 90){         //Upper
             out[i] = part + 122;
         } else if (part >=97 && part <= 122){  //Lower
             out[i] = part + 116;
         } else if (part >=48 && part <= 57){   //Number
             out[i] = part + 113;
         } else if (indexOf(symb,input.charAt(i)) != -1){
             out[i] = hexToInt(symbnum[indexOf(symb,input.charAt(i))]);
         } else if (part == 91){ //[
             out[i] = -1;
             i++;
             StringBuilder sb = new StringBuilder(input.substring(i,input.indexOf(']', i)));
             if (sb.length() % 2 != 0){
                 //textSentinel
                 System.out.println("Erroneous hex code: " + sb.toString() + " -- " + input);
                 sb.append("0");
             }
             for (;sb.length() > 0;remove++,i+=2){
                 out[i] = Integer.parseInt(sb.substring(0,2),16);
                 out[i+1] = -1;
                 sb.delete(0,2);
             }
             out[i] = -1;
             remove += 2; //for the [ and ]
         } else{
             //textSentinel
             System.out.println("Unknown char encoding dropped: "+input.charAt(i) + " " + input);
             out[i] = hexToInt("00");
         }
     }

     if (remove > 0){
         int[] out2 = new int[out.length-remove];
         for (int i=0,j=0;i<out.length;i++){
             if (out[i] == -1) continue;
             out2[j] = out[i];
             j++;
         }
         return out2;
     }
     return out;
 }

  private byte[] hexStringToByteArray(String in){
      if (in.length() % 2 != 0 || in.length() == 0){
          System.out.println("NOTE: There was likely a hex string parsing error due to an odd String length.");
      }
      byte[] out = new byte[in.length()/2];
      for (int i=0;i<out.length;i++){
          out[i] = intToByte(hexToInt(in.substring(i*2,i*2+2)));
      }
      return out;
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

 private int addy(String inhex){ //119044
     String hex;
     if (inhex.length()%2 == 1){
         hex = '0' + inhex;
     } else{
         hex = inhex;
     }
     int result = 0;
     int mult = 1;
     for (int i=hex.length();i>0;i-=2,mult *= 256){
         result += mult * hexToInt(hex.substring(i-2,i));
     }
     return result;
 }

 private void setActive(boolean chox){
     miClose.setEnabled(chox);
     miSave.setEnabled(chox);
     miSaveAs.setEnabled(chox);

     miLow.setEnabled(chox);
     miMedium.setEnabled(chox);
     miHigh.setEnabled(chox);
     miArty.setEnabled(chox);
     miRandom.setEnabled(chox);
     miClear.setEnabled(chox);

     bSave.setEnabled(chox);
     bPrint.setEnabled(chox);

     cbStarter1.setEnabled(chox);
     cbStarter2.setEnabled(chox);
     cbStarter3.setEnabled(chox);
     cbItem.setEnabled(chox);
     cbEnsureBasic.setEnabled(chox);
     bRandomStarters.setEnabled(chox);
     cbNoLegends.setEnabled(chox);

     rbUnchanged.setEnabled(chox);
     rbSubs.setEnabled(chox);
     rbRandom.setEnabled(chox);
     rbGlobal.setEnabled(chox);
     cbUnique.setEnabled(chox);
          cbUniqueLegends.setEnabled(false);
     cbNoLegendsWild.setEnabled(chox);
     cbKeepForm.setEnabled(chox);
     cbEnsureAll.setEnabled(chox);

     rbTrainerUnchanged.setEnabled(chox);
     rbTrainerGlobal.setEnabled(chox);
     rbTrainerRandom.setEnabled(chox);
     cbTrainerNames.setEnabled(chox);
     cbRivals.setEnabled(chox);
     cbRandTrainerUseItems.setEnabled(chox);
     cbRandTrainerHeldItems.setEnabled(chox);
     cbNoTrainerLegends.setEnabled(chox);
     cbBattleFrontier.setEnabled(chox);

     cbTMs.setEnabled(chox);
     cbMovesets.setEnabled(chox);
     cbRandStats.setEnabled(chox);
     cbRandTypes.setEnabled(chox);
     cbRandTMLearn.setEnabled(chox);
     cbRandAbilities.setEnabled(chox);
     cbFixEvos.setEnabled(chox);
     cbRandHeldItems.setEnabled(chox);
     cbRandPickup.setEnabled(chox);
     cbEnsureAttacks.setEnabled(chox);
     cbRandTrades.setEnabled(chox);
     cbMetronome.setEnabled(chox);
     cbHeartScale.setEnabled(chox);
     cbMultTMs.setEnabled(chox);
     cbRandFieldItems.setEnabled(chox);
     cbNatlDex.setEnabled(chox);

     rbPalNoChange.setEnabled(chox);
     rbPalType.setEnabled(chox);
     rbPalRandom.setEnabled(chox);
     cbPalPrimary.setEnabled(chox);
     cbKeepColors.setEnabled(chox);
     cbShinyNorm.setEnabled(chox);

     cbStarter1.setSelectedIndex(0);
     cbStarter2.setSelectedIndex(0);
     cbStarter3.setSelectedIndex(0);
     cbItem.setSelectedIndex(0);

     if (chox){
         cbStarter1.setModel(cbmPokemon1);
         cbStarter2.setModel(cbmPokemon2);
         cbStarter3.setModel(cbmPokemon3);
         cbItem.setModel(cbmItems);
     } else{
         cbStarter1.setModel(cbmEmpty);
         cbStarter2.setModel(cbmEmpty);
         cbStarter3.setModel(cbmEmpty);
         cbItem.setModel(cbmEmpty);
     }

     rbUnchanged.setSelected(true);
     rbTrainerUnchanged.setSelected(true);
     rbPalNoChange.setSelected(true);
 }

 private void setTrainerChoices(boolean chox){
     cbNoTrainerLegends.setEnabled(chox);
     cbBattleFrontier.setEnabled(chox);
     cbRivals.setEnabled(chox);
     cbNoTrainerLegends.setSelected(false);
     cbBattleFrontier.setSelected(false);
     cbRivals.setSelected(false);
 }

 private void setWildChoices(boolean chox){
     cbEnsureAll.setEnabled(chox);
     cbKeepForm.setEnabled(chox);
     cbNoLegendsWild.setEnabled(chox);
     cbEnsureAll.setSelected(false);
     cbKeepForm.setSelected(false);
     cbNoLegendsWild.setSelected(false);
 }

 private void setPaletteChoices(boolean chox){
     cbPalPrimary.setEnabled(chox);
     cbKeepColors.setEnabled(chox);
     cbShinyNorm.setEnabled(chox);
     cbPalPrimary.setSelected(false);
     cbKeepColors.setSelected(false);
     cbShinyNorm.setSelected(false);
 }

 private void setSelected(boolean[] chox, int[] radio){
     cbStarter1.setSelectedIndex(0);
     cbStarter2.setSelectedIndex(0);
     cbStarter3.setSelectedIndex(0);
     cbItem.setSelectedIndex(0);
     cbEnsureBasic.setSelected(chox[0]);
     cbNoLegends.setSelected(chox[1]);

     switch (radio[0]){
         case 0:
             rbUnchanged.setSelected(true);
             break;
         case 1:
             rbGlobal.setSelected(true);
             break;
         case 2:
             rbSubs.setSelected(true);
             break;
         default:
             rbRandom.setSelected(true);
     }
     cbUnique.setSelected(chox[2]);
          cbUniqueLegends.setSelected(chox[3]);
     cbEnsureAll.setSelected(chox[4]);
     cbKeepForm.setSelected(chox[5]);
     cbNoLegendsWild.setSelected(chox[6]);
     cbRandTrades.setSelected(chox[7]);
     
    switch (radio[1]){
         case 0:
             rbTrainerUnchanged.setSelected(true);
             break;
         case 1:
             rbTrainerGlobal.setSelected(true);
             break;
         default:
             rbTrainerRandom.setSelected(true);
     }
     cbTrainerNames.setSelected(chox[8]);
     cbRandTrainerHeldItems.setSelected(chox[9]);
     cbRandTrainerUseItems.setSelected(chox[10]);
     cbRivals.setSelected(chox[11]);
     cbNoTrainerLegends.setSelected(chox[12]);
     cbBattleFrontier.setSelected(chox[13]);

     cbRandStats.setSelected(chox[14]);
     cbRandTypes.setSelected(chox[15]);
     cbRandAbilities.setSelected(chox[16]);
     cbRandHeldItems.setSelected(chox[17]);
     cbTMs.setSelected(chox[18]);
     cbRandTMLearn.setSelected(chox[19]);
     cbMovesets.setSelected(chox[20]);
     cbEnsureAttacks.setSelected(chox[21]);
     cbMetronome.setSelected(chox[22]);

     switch (radio[2]){
         case 0:
             rbPalNoChange.setSelected(true);
             break;
         case 1:
             rbPalType.setSelected(true);
             break;
         default:
             rbPalRandom.setSelected(true);
     }
     cbPalPrimary.setSelected(chox[23]);
     cbKeepColors.setSelected(chox[24]);
     cbShinyNorm.setSelected(chox[25]);

     cbFixEvos.setSelected(chox[26]);
     cbHeartScale.setSelected(chox[27]);
     cbRandPickup.setSelected(chox[28]);
     cbRandFieldItems.setSelected(chox[29]);
     cbMultTMs.setSelected(chox[30]);
     cbNatlDex.setSelected(chox[31]);
 }

        //===========================ROMFILTER=========================

 public class ROMFilter extends javax.swing.filechooser.FileFilter{
     private String description;
     private String[] types;

     public ROMFilter(String descr, String...t){
         description=descr;
         types = t;
     }

     public boolean accept(File f){
         String fileName = f.getName();
         if (fileName.contains(".")){
             String type = fileName.substring(fileName.lastIndexOf(".")+1,fileName.length());
             for (int i=0;i<types.length;i++){
                 if (types[i].equals(type.toLowerCase())) return true;}
             return false;
         }
         return true;
     }

     public String getDescription(){
         return description;
     }
 }


    // Variables declaration - do not modify//GEN-BEGIN:variables
    private javax.swing.JButton bOpen;
    private javax.swing.JButton bPrint;
    private javax.swing.JButton bRandomStarters;
    private javax.swing.JButton bSave;
    private javax.swing.ButtonGroup bgPalette;
    private javax.swing.ButtonGroup bgTrainer;
    private javax.swing.ButtonGroup bgWildPokemon;
    private javax.swing.JCheckBox cbBattleFrontier;
    private javax.swing.JCheckBox cbEnsureAll;
    private javax.swing.JCheckBox cbEnsureAttacks;
    private javax.swing.JCheckBox cbEnsureBasic;
    private javax.swing.JCheckBox cbFixEvos;
    private javax.swing.JCheckBox cbHeartScale;
    private javax.swing.JComboBox cbItem;
    private javax.swing.JCheckBox cbKeepColors;
    private javax.swing.JCheckBox cbKeepForm;
    private javax.swing.JCheckBox cbMetronome;
    private javax.swing.JCheckBox cbMovesets;
    private javax.swing.JCheckBox cbMultTMs;
    private javax.swing.JCheckBox cbNatlDex;
    private javax.swing.JCheckBox cbNoLegends;
    private javax.swing.JCheckBox cbNoLegendsWild;
    private javax.swing.JCheckBox cbNoTrainerLegends;
    private javax.swing.JCheckBox cbPalPrimary;
    private javax.swing.JCheckBox cbRandAbilities;
    private javax.swing.JCheckBox cbRandFieldItems;
    private javax.swing.JCheckBox cbRandHeldItems;
    private javax.swing.JCheckBox cbRandPickup;
    private javax.swing.JCheckBox cbRandStats;
    private javax.swing.JCheckBox cbRandTMLearn;
    private javax.swing.JCheckBox cbRandTrades;
    private javax.swing.JCheckBox cbRandTrainerHeldItems;
    private javax.swing.JCheckBox cbRandTrainerUseItems;
    private javax.swing.JCheckBox cbRandTypes;
    private javax.swing.JCheckBox cbRivals;
    private javax.swing.JCheckBox cbShinyNorm;
    private javax.swing.JComboBox cbStarter1;
    private javax.swing.JComboBox cbStarter2;
    private javax.swing.JComboBox cbStarter3;
    private javax.swing.JCheckBox cbTMs;
    private javax.swing.JCheckBox cbTrainerNames;
    private javax.swing.JCheckBox cbUnique;
    private javax.swing.JCheckBox cbUniqueLegends;
    private javax.swing.JLabel jLabel1;
    private javax.swing.JLabel jLabel2;
    private javax.swing.JLabel jLabel3;
    private javax.swing.JLabel jLabel4;
    private javax.swing.JLabel jLabel5;
    private javax.swing.JLabel jLabel6;
    private javax.swing.JLabel jLabel7;
    private javax.swing.JLabel jLabel8;
    private javax.swing.JLabel jLabel9;
    private javax.swing.JPanel jPanel1;
    private javax.swing.JPanel jPanel2;
    private javax.swing.JPanel jPanel3;
    private javax.swing.JPanel jPanel4;
    private javax.swing.JPanel jPanel5;
    private javax.swing.JPanel jPanel6;
    private javax.swing.JPanel jPanel7;
    private javax.swing.JPanel jPanel8;
    private javax.swing.JScrollPane jScrollPane1;
    private javax.swing.JPopupMenu.Separator jSeparator1;
    private javax.swing.JSeparator jSeparator2;
    private javax.swing.JSeparator jSeparator3;
    private javax.swing.JPopupMenu.Separator jSeparator4;
    private javax.swing.JLabel lStarters;
    private javax.swing.JMenu mQuickSettings;
    private javax.swing.JPanel mainPanel;
    private javax.swing.JMenuBar menuBar;
    private javax.swing.JMenuItem miArty;
    private javax.swing.JMenuItem miClear;
    private javax.swing.JMenuItem miClose;
    private javax.swing.JMenuItem miHigh;
    private javax.swing.JMenuItem miLow;
    private javax.swing.JMenuItem miMedium;
    private javax.swing.JMenuItem miOpen;
    private javax.swing.JMenuItem miRandom;
    private javax.swing.JMenuItem miSave;
    private javax.swing.JMenuItem miSaveAs;
    private javax.swing.JPanel pStarter;
    private javax.swing.JRadioButton rbGlobal;
    private javax.swing.JRadioButton rbPalNoChange;
    private javax.swing.JRadioButton rbPalRandom;
    private javax.swing.JRadioButton rbPalType;
    private javax.swing.JRadioButton rbRandom;
    private javax.swing.JRadioButton rbSubs;
    private javax.swing.JRadioButton rbTrainerGlobal;
    private javax.swing.JRadioButton rbTrainerRandom;
    private javax.swing.JRadioButton rbTrainerUnchanged;
    private javax.swing.JRadioButton rbUnchanged;
    private javax.swing.JTextArea taWild;
    // End of variables declaration//GEN-END:variables

    private static String romPath = "";
    private static byte[] rom;
    private static boolean saved = true;
    private final String[] empty = {"---"};

    //bridge between dex -> coded
    private final int[] dexToCoded = {0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,
        31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,65,66,67,68,
        69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,99,100,101,102,103,104,
        105,106,107,108,109,110,111,112,113,114,115,116,117,118,119,120,121,122,123,124,125,126,127,128,129,130,131,132,
        133,134,135,136,137,138,139,140,141,142,143,144,145,146,147,148,149,150,151,152,153,154,155,156,157,158,159,160,
        161,162,163,164,165,166,167,168,169,170,171,172,173,174,175,176,177,178,179,180,181,182,183,184,185,186,187,188,
        189,190,191,192,193,194,195,196,197,198,199,200,201,202,203,204,205,206,207,208,209,210,211,212,213,214,215,216,
        217,218,219,220,221,222,223,224,225,226,227,228,229,230,231,232,233,234,235,236,237,238,239,240,241,242,243,244,
        245,246,247,248,249,250,251,252,253,254,255,256,257,258,259,260,261,262,263,264,265,266,267,268,269,270,271,272,
        273,274,275,
        279,280,284,285,367,368,369,286,287,281,282,339,340,341,276,277,278,345,346,347,310,311,325,295,290,291,297,330,
        357,358,359,331,332,312,313,328,329,361,362,338,342,343,305,306,288,289,314,315,296,326,327,283,307,308,309,319,
        320,333,334,355,354,323,324,298,299,301,302,293,294,363,364,365,366,303,304,360,292,352,353,336,337,344,386,351,
        335,321,322,316,317,318,348,349,350,356,300,370,371,372,373,374,375,376,377,378,382,383,379,380,381,384,385};
//        coded to dex
//        290,291,292,276,277,285,286,327,278,279,283,284,320,321,300,301,352,343,344,299,324,302,339,340,370,
//        341,342,349,350,318,319,328,329,330,296,297,309,310,322,323,363,364,365,331,332,361,362,337,338,298,325,326,311,
//        312,303,307,308,333,334,360,355,356,315,287,288,289,316,317,357,293,294,295,366,367,368,359,353,354,336,335,369,
//        304,305,306,351,313,314,345,346,347,348,280,281,282,371,372,373,374,375,376,377,378,379,382,383,384,380,381,385,
//        386,358

    //dex
    

    
    //coded
    private final int[] specialEvos = {106,107,134,135,136,196,197,237,374,375};

    private DefaultComboBoxModel cbmEmpty = new DefaultComboBoxModel(empty);
    ArrayKeeper ak = new ArrayKeeper();
    private DefaultComboBoxModel cbmPokemon1 = new DefaultComboBoxModel(ak.getPokemonMenuList());
    private DefaultComboBoxModel cbmPokemon2 = new DefaultComboBoxModel(ak.getPokemonMenuList());
    private DefaultComboBoxModel cbmPokemon3 = new DefaultComboBoxModel(ak.getPokemonMenuList());
    private DefaultComboBoxModel cbmItems = new DefaultComboBoxModel(new ArrayKeeper().getItemList());

    private JDialog aboutBox;
    private final int PREV_PAL_CHANCE = 80; // % (out of 100) that an evolution retains its previous form's palette
}
