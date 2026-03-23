# Bring Clo Clothing into Unreal Engine

## Build Clothing Item in Clo
1. Open Clo.

## Option A: Import Metahuman from Unreal
1. In the main menu, select **File** -> **Import** -> **DNA**.
1. In the **MetaHuman DNA Importer** dialog...
   1. Click the folder icon to the right of **Body** and browse to the location of your exported MetaHuman's *body.dna* file.
   1. Click the folder icon to the right of **Head** and browse to the location of your exported MetaHuman's *head.dna* file.
   1. Click the **OK** button.

## Option B: Use Existing Metahuman in Clo
1. Choose your avatar (e.g. f_med_nrw_combined.avte)

## Design Your Clothing Item in Clo
1. Add (or build) a garment using patterns in Clo.
1. Select patterns: 
   1. Reduce mesh density. (For real-time experiences like games in Unreal Engine, the recommendation is *18mm*.)
   1. Set **Add’l Thickness – Collision** to *5mm*
   1. Set **Add’l Thickness – Rendering** to *3mm*
1. Fit clothes to avatar
1. Turn on simulation
1. In the main menu, select **Materials/UV** -> **UV Editor**.
1. In the **UV Editor** tab, select all of the patterns, and click the **Auto UV Packaging** button in the toolbar to the right.
1. In the **UV Packing** dialog...
   1. Set **Padding** to *0.006*.
   1. Click the **Apply** button and the click the **Close** button.
1. In the main menu, select **File** -> **Export** -> **USD**
1. In the **Export USD** dialog...
   1. Unselect **Avatar** and **SceneAndProp**.
   1. Select **Multiple Objects**.
   1. Select **Thick**.
   1. Select **Unified UV Coordinates**.
   1. For **Image Size**, enter *4096*.
   1. Select **Include Garment Simulation Data**.
   1. Click the **OK** button.

## Create a Cloth Asset
1. Open your Unreal Engine project.
1. In the **Content Drawer**...
   1. create a folder named *Outfits*.
   1. In the **Outfits** folder, create a folder named the same as the USD file you exported out of Clo. Going forward, this will be known as your *working folder* for this clothing item.
   1. Double-click into your new *working item* folder, click the **Import** button, and browse to the location of your USD file.
   1. A new folder will be created with the same name as the parent *working folder*. This new folder will contain materials, static meshes, and textures for this clothing item.
1. In your *working folder* (the parent folder - not the child folder of the same name), click the **Add button**, and select **Physics** -> **Cloth Asset**. Name this file *CA_[name of your folder]*. (A Dataflow Asset with a Default Node Graph Preset is automatically created.)

## Apply Skin Weights
1. Double-click your new Cloth Asset to open it's Dataflow Node Graph.
1. Select the **USDImport** node and, in the **Node Details** window, browse for USD File.
1. [Optional] Use **TransformPositions** node to modify translation, rotation, and scale.
1. In the **TransferSkinWeights** node, select the simulation mesh (e.g. f_med_nrw_body_Physics)
1. In the **WeightMap_MaxDistance**, paint weight maps to determine how far the cloth can move away from the animated skinned position.
1. In the **SimulationMaxDistanceConfig** node, change the Max Distance property to determine how far the cloth can move away from the animated skinned position. (e.g. **Lo** to *0.0* and **Hi** to *40.0*.)
1. [Optional] Use the **SimulationStretchConfig** and **SimulationBendingConfig** nodes to override the Clo parameters.
1. In the **SetPhysicsAsset** node, select a physics asset (e.g. f_med_nrw_body_Physics)
1. In the **SimulationSelfCollisionSpheresConfig** node, you can set Dataflow to Active.
1. In the **SimulationCollisionConfig** node, turn on the blue toggle for Collision Thickness (which then uses the values from the USD file).
1. Click Save and close the window.
1. Click Save All in the Content Drawer.

## Create Outfit Asset
1. In Content Browser, navigate to your working folder (e.g. /Game/Outfits/Blouse01/Blouse01/).
1. Right-click in the folder → **Physics** → **Outfit Asset**.
1. Name it OA_BlouseSkirt.
1. In the dialog, select **Resizable Outfit** template and click **OK**.
1. Double-click your Outfit Asset (e.g. OA_Blouse) to open the Dataflow Editor.
1. In the main menu, click the three dots next to **Evaluate Dataflow Graph** and select **Manual Graph Evaluation** for faster editing.
1. In the **Node Details** window, check the Sized Outfit Source checkbox.
1. Click the plus (+) icon to add an index entry for each cloth asset you have (e.g., blouse, skirt).
1. For each entry...
   1. In **Source Asset** dropdown, select the Cloth Asset.
   1. In **Size Name**, enter the corresponding body name (e.g., bodyShapeG or your target skeletal mesh name).
   1. Assign your skeletal mesh to Source Body Parts (e.g., the imported USD skeletal mesh).
1. Click **Evaluate Dataflow Graph** to bake the outfit asset and wait for progress bar completion (can take a few minutes).
1. After evaluation completes, re-enable **Automatic Graph Evaluation** from the three dots menu.

## Apply to Metahuman
1. Double-click to open the Metahuman blueprint.
1. In the **Components** tab, select the *Body* node, click the **Add** button and choose a **Chaos Cloth** component.
1. Select the Chaos Cloth component, in the properties window, assign the Cloth Asset you made above.
1. Compile and save

## References
- [YouTube: USD to Unreal Engine Chaos Cloth Workflow](https://www.youtube.com/watch?v=Nf6OUDoPCPs)
- [YouTube: How to use MetaHumans in CLO](https://www.youtube.com/watch?v=LCtRMiPGLgc)
- [YouTube: #UE5 Series: Parametric Outfits | Marvelous Designer to Unreal Engine](https://www.youtube.com/watch?v=e0MECCKUq7o)
- [CLO to MetaHuman: USD Garment Integration Workflow](https://support.clo3d.com/hc/en-us/articles/53322960594969-CLO-to-MetaHuman-USD-Garment-Integration-Workflow)
- [Creating Parametric Clothing for Fab](https://dev.epicgames.com/documentation/en-us/unreal-engine/creating-parametric-clothing-for-fab)
- [Panel Cloth Editor](https://dev.epicgames.com/community/learning/tutorials/pv7x/unreal-engine-cloth-panel-editor)
