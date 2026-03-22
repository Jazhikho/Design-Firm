# Create an Unreal Metahuman Avatar to Use in Clo
## Requirements
- Unreal Engine 5.6+

## Create an Unreal Metahuman
1. In Epic Games Launcher, ensure **Metahuman Creator Core Data** is installed
1. Open a new or existing Unreal Engine project
1. In the main menu, select **Edit** -> **Plugins**
1. Ensure **Chaos Cloth** and **MetaHuman Creator** plugins are enabled. If they were not already enabled, you will need to restart Unreal Engine.
1. In the **Content Drawer**, select the *Content* folder. Click the **Add** button and select **MetaHuman** -> **MetaHuman Character**. Name it whatever you want (e.g. *Character01* or *Mary*).
1. Double-click your Metahuman to open it in the Metahuman editor. Select your **Preset** and edit any **Body** or **Head** parameters to build the metahuman avatar you want.
1. Click the **Hair & Clothing** tab. Scroll down to the bottom of the left-hand pane to find **Outfit Clothing**. Select the outfit and click the **Remove** button.
1. In the top menu, click **Create Full Rig**.
1. Click the **Assembly** tab.
   1. Under **Assembly Selection** header, change **Assembly** to **DCC Export**.
   1. Under **Targets** header, change **Root Directory** to the desired folder (e.g. "C:/Metahuman Exports").
   2. At the bottom of the left-hand pane, click the **Assemble** button.

## References
- [YouTube: USD to Unreal Engine Chaos Cloth Workflow](https://www.youtube.com/watch?v=Nf6OUDoPCPs)
- [YouTube: How to use MetaHumans in CLO](https://www.youtube.com/watch?v=LCtRMiPGLgc)
