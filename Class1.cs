using System;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;

public class HitmanMod : Script
{
    // Define the base pay and pay range
    private static int basePay = 100000;
    private static int payRange = 70000;

    private int reward = 0;

    // Define the hitman contract pickup location
    private Vector3 contractLocation = new Vector3(1704.875f, 3310.366f, 10.0f);

    // Define blip sprites
    private Blip hitmanBlip;
    private Blip targetBlip;

    // Keep track of the active contract
    private Ped target;

    // Constructor
    public HitmanMod()
    {
        Tick += OnTick;
        Interval = 1000;

        // Create blip for contract pickup location
        hitmanBlip = World.CreateBlip(contractLocation);
        hitmanBlip.Sprite = BlipSprite.Crosshair;
        hitmanBlip.Color = BlipColor.Red;
        hitmanBlip.Name = "Hitman Contract Pickup";

        Notification.Show("Loaded successfully");
    }

    // Main tick function
    private void OnTick(object sender, EventArgs e)
    {
        // Check if player is near the contract pickup location
        if (Game.Player.Character.Position.DistanceToSquared(contractLocation) < 50.0f * 50.0f)
        {
            // Display prompt to accept the contract
            Notification.Show($"Press O to check for a new contract");

            // Check if player pressed the pickup key
            if (Game.IsKeyPressed(Keys.O))
            {
                GenerateContract();
            }

            if (target != null)
            {
                Notification.Show("Target Active");

                // Check if target is dead
                if (target.IsDead)
                {
                    Notification.Show("Target Dead");
                    OnTargetKilled();
                }
            }
        }
    }

    // Function to handle accepting the contract
    private void GenerateContract()
    {
        //Randomize pay increase or decrease
        Random rnd = new Random();
        int rewardMod = rnd.Next(-payRange, payRange);

        // Pay the player
        reward = basePay + rewardMod;

        // Display notification on the phone
        string message = $"New Hitman Contract\nPay: ${reward}\nPress Y or N to accept contract";
        Notification.Show(message);

        // Define the range for random X and Y coordinates
        float range = 150.0f; // Half of the total range (300x300 area)
        Random random = new Random();

        // Calculate random X and Y offsets within the range
        float offsetX = random.Next((int)-range, (int)range);
        float offsetY = random.Next((int)-range, (int)range);

        // Calculate the position for the generated target NPC
        Vector3 targetPosition = contractLocation + new Vector3(offsetX, offsetY, 0);

        // Ensure Z coordinate is at ground level
        targetPosition.Z = World.GetGroundHeight(targetPosition);

        // Create the target NPC at the calculated position
        if (target != null && target.Exists()) // Cleanup previous target
        {
            target.Delete();
            targetBlip.Delete();
        }
        target = World.CreatePed(PedHash.Abigail, targetPosition);
        target.IsPersistent = true; // Make the target persistent

        // Create blip for the target
        targetBlip = target.AddBlip();
        targetBlip.Sprite = BlipSprite.Enemy;
        targetBlip.Color = BlipColor.Red;
        targetBlip.Name = "Target";
    }

    // Event handler for when the target is killed
    private void OnTargetKilled()
    {
        // Add reward to money
        Game.Player.Money += reward;

        // Display notification
        Notification.Show($"Contract completed! You received ${reward}");

        // Clean up
        if (target != null && target.Exists())
        {
            target.Delete();
            targetBlip.Delete();
        }
        target = null;
    }
}
