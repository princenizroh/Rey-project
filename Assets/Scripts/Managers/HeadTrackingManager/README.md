# Head Tracking Setup Guide

## 📁 File Structure
```
Scripts/
└── Managers/
    └── HeadTrackingManager/
        └── HeadTrackingManager.cs
```

## 🔧 Scene Setup Instructions

### Step 1: Setup Character Constraints
For each character (Mother, Father, Bidan):

1. **Select Character GameObject**
2. **Add Rig Builder** component (if not already exists)
3. **Create child object** called "Rig1"
4. **Add Multi-Aim Constraint** to Rig1
5. **Configure constraint**:
   - Constrained Object: `mixamorig:Head`
   - Aim Axis: Z Forward
   - Up Axis: Y Up
   - Source Objects (5):
     - [0] CameraTarget (Empty GameObject)
     - [1] MotherTarget (Empty GameObject) 
     - [2] FatherTarget (Empty GameObject)
     - [3] BabyTarget (Empty GameObject)
     - [4] BidanTarget (Empty GameObject)

### Step 2: Create Target Objects
Create empty GameObjects for each target:
1. **CameraTarget** (position at camera)
2. **MotherTarget** (position at Mother's head)
3. **FatherTarget** (position at Father's head)
4. **BabyTarget** (position at Baby)
5. **BidanTarget** (position at Bidan's head)

### Step 3: Setup HeadTrackingManager
1. **Create empty GameObject** called "HeadTrackingManager"
2. **Add HeadTrackingManager script**
3. **Configure Character Head Rigs array**:
   - Size: 3 (Mother, Father, Bidan)
   - For each element:
     - Character Type: Mother/Father/Bidan
     - Head Constraint: Drag Multi-Aim Constraint component
     - Assign all target references
4. **Configure Global Target References**:
   - Main Camera: Drag main camera
   - Character transforms for each character

## 🎮 Usage in NarratorDay Scripts

### Basic Usage:
```csharp
// All characters look at baby (camera POV)
SetHeadTargetCamera(CharacterType.Mother);
SetHeadTargetCamera(CharacterType.Father);
SetHeadTargetCamera(CharacterType.Bidan);

// Characters look at each other
SetHeadTargetFather(CharacterType.Mother);  // Mother looks at Father
SetHeadTargetMother(CharacterType.Father);  // Father looks at Mother

// Disable tracking
DisableHeadTracking(CharacterType.Bidan);
EnableGlobalHeadTracking(false);  // Disable all
```

### Available Methods:
- `SetHeadTargetCamera(character)`
- `SetHeadTargetMother(character)`  
- `SetHeadTargetFather(character)`
- `SetHeadTargetBaby(character)`
- `SetHeadTargetBidan(character)`
- `DisableHeadTracking(character)`
- `EnableGlobalHeadTracking(bool)`
- `SetMultipleHeadTargetsCamera(CharacterType[])`

## 🎯 Example Usage in NarratorDay1:

```csharp
// Scene start - everyone focuses on baby
SetHeadTargetCamera(CharacterType.Mother);
SetHeadTargetCamera(CharacterType.Father);
SetHeadTargetCamera(CharacterType.Bidan);

// Dialog moment - parents look at each other
SetHeadTargetFather(CharacterType.Mother);
SetHeadTargetMother(CharacterType.Father);

// Back to baby focus
SetHeadTargetCamera(CharacterType.Mother);
SetHeadTargetCamera(CharacterType.Father);

// Movement scene - disable for natural look
DisableHeadTracking(CharacterType.Bidan);

// Scene end - disable all
EnableGlobalHeadTracking(false);
```

## 🔍 Debug Features:
- Context menu options in HeadTrackingManager
- Console logs for tracking state changes
- Test methods for quick setup validation

## 📝 Notes:
- Each character needs their own Multi-Aim Constraint
- Source object weights are managed automatically by script
- Target positions update dynamically (camera follows player)
- Works alongside existing animation system
- Performance optimized for real-time updates
