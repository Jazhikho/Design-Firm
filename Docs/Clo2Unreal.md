# Clo 3D
1. Choose f_med_nrw_combined.avte
1. Fit clothes to avatar
1. Select patterns: 
   1. reduce mesh density (18mm?)
   1. set **Add’l thickness – collision** to *5mm*
   1. set **Add’l thickness – rendering** to *3mm*
1. Turn on simulation
1. **File** -> **Export** -> **USD**
1. In Export USD:
   1. Unselect **Select All Avatars**
   1. Select **Multiple Objects**
   1. Select **Thick**
   1. Select **Unified UV Coordinates**
   1. Select **Include Garment Simulation Data**
 
# Unreal Engine
1. Install Substance 3D plugin (free from Fab)
1. Install CloLiveSync from: LiveSync version archive – Help Center - CONNECT (I don't think this is necessary for this workflow)
1. In the main menu, go to **Edit** -> **Project Settings…**
1. Search for ‘transparency’ and enable **Enable Order Independent Transparency (Experimental)**.
 
## Create Cloth Asset
1. Add a Cloth Asset (a Dataflow Asset with a Default Node Graph Preset is automatically created)
1. Double-click to open the Cloth Asset in Dataflow Node Graph
1. Select **USDImport** node and, in properties window, browse for USD File.
1. [Optional] Use **TransformPositions** node to modify translation, rotation, and scale.
1. In **TransferSkinWeights** node, select the simulation mesh (e.g. f_med_nrw_body_Physics)
1. In the **AddWeightMap_MaxDistance**, paint weight maps to determine how far the cloth can move away from the animated skinned position.
1. In the **SimulationMaxDistanceConfig** node, change the Max Distance property to determine how far the cloth can move away from the animated skinned position.
1. [Optional] Use the **SimulationStretchConfig** and **SimulationBendingConfig** nodes to override the Clo parameters.
1. In the **SetPhysicsAsset** node, select a physics asset (e.g. f_med_nrw_body_Physics)
1. In the **SimulationSelfCollisionSpheresConfig** node, you can set Dataflow to Active.
1. In the **SimulationCollisionConfig** node, turn on the blue toggle for Collision Thickness (which then uses the values from the USD file).
1. Click Save and close the window.
1. Click Save All in the Content Drawer.
1. Apply to Metahuman
1. Double-click to open the Metahuman blueprint.
1. In the **Components** tab, select the *Body* node, click the **Add** button and choose a **Chaos Cloth** component.
1. Select the Chaos Cloth component, in the properties window, assign the Cloth Asset you made above.
1. Compile and save

# References
- [YouTube: USD to Unreal Engine Chaos Cloth Workflow](https://www.youtube.com/watch?v=Nf6OUDoPCPs)
- [YouTube: How to use MetaHumans in CLO](https://www.youtube.com/watch?v=LCtRMiPGLgc)
