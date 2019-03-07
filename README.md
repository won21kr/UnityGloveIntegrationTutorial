# UnityGloveIntegrationTutorial

StretchSense Glove Setup Tutorial using Unity as a Case Study

This tutorial takes you through the steps of integrating a StretchSense BLE Demonstration Glove hardware into Unity and using it to animate a hand avatar that copies the gloved hand. While we use Unity in this tutorial, integrating a StretchSense BLE Demonstration Glove into your animation or mocap software environment follows the same fundamental steps no matter what platform you use. We will continue to develop and roll out support for specific platforms, but for those who cannot wait and want to take matters into their own hands and get started straight away, this tutorial will serve as a guide to what is required.  

This tutorial focuses on creating a basic Unity glove application which can connect to a StretchSense Demonstration Glove via BLE, communicate with the glove, calibrate the raw sensor data and convert it to inverse kinematic (IK) data, and using this IK data to animate a hand avatar.

StretchSense sensors are accurate, stable and repeatable. This makes it possible to easily train the glove to mimic real hand motion.

## Prerequisites

<ol>
<li>Any Unity asset which has bone structure. Because this tutorial is about a smart glove it is natural we start with a hand, but note the process below is identical for any morphable avatar. For this tutorial we have used Realistic Hand available from the Unity Asset Store for US$29.99 (https://assetstore.unity.com/packages/3d/characters/realistic-hands-animated-for-vr-102870)</li>
<li>A BLE library that works for the OS of your choice. We have used a BLE library that provides support for both Android & iOS that is available from the Unity Asset Store for US$20 (https://assetstore.unity.com/packages/tools/network/bluetooth-le-for-ios-tvos-and-android-26661)</li>
</ol>

### Instructions
#### Initial Setup

<ol>
<li>Create a new Project and import the target avatar/asset</li>
<ol>
<li>The asset can be anything which we want to control using mocap data from the StretchSense glove. See this short video.</li>
<li><screenshot "new project"> <screen shot "import avatar"> <screenshot "heirarchy with asset imported"></li>
</ol>
<li>Import the BLE script for handling BLE communications (see Prerequisites fo rthe exact library we use). See this video <screenshot "import BLE script"><screenshot "heirarchy with script imported"></li>
<ol>
<li>
If you already have a BLE script/plugin, create a Plugin folder within your Unity Project and copy the script files to that location.
</li>
</ol>
<li>The tutorial project has following 4 main components <insert flow diagram here>:</li><br>
<ol>
<li><strong>GloveConnectionManager: </strong> This component handles establishing the BLE connection with the glove. It scans for available BLE devices and prompts you to connect once a StretchSense device is found. Key steps in this process are:</li>
<ol>
<li>Scan for devices</li>
<li>Prompt to connect to found device(s)</li>
<li>Subscribe to services</li>
<li>Enable Notifications</li>
</ol><br>
<li><strong>GloveDataProcessor:</strong> Once the connection is established by the <strong>GloveConnectionManager</strong> sends the raw sensor data received data received to GloveDataProcessor. If the glove has not yet been calibrated, the raw sensor data is used by the CalbirationAlgorithm component when the user initiates the calibration process. Once the glove has been calibrated, GloveDataProcessor will transform the raw sensor data into IK data to be sent to the GloveController component. </li><br>
<li><strong>GloveController: </strong>This component is responsible for updating the avatar animation. When the user initiates the calibration process, the Glove controller will step through a sequence of hand reference poses and ask the user to mimic the pose and tap the screen to confirm before moving to the next pose. Once the glove is calibrated, GloveController will transform raw sensor data into IK data and use this IK data to update the hand animation in realtime. </li><br>
<li><strong>CalibrationAlgorithm</strong> : This component is where we define our sequence of reference poses, capture data from the user in these reference poses, and perform a least squares solve to determine the coefficient matrix required to convert raw sensor data into the desired IK data. For each reference pose in this tutorial, we define the angles of all of the joints in the hand for each pose. During calibration, the hand avatar will display the reference pose for the user to copy, and then ask the user to tap the screen to confirm. A snapshot of the raw sensor outputs from the glove is then associated with the joint angles of the reference pose. Once several snapshots have been taken, SolveLeastSquares is used to determine a matrix of coefficients that will transform the raw sensor data into IK data directly.</li><br>

</ol>
</ol>

Video Run Through : CodeRunThrough

#### Configuring the Reference Poses   

<i>When deciding the number of poses and hand shapes to use, the goal is to make sure each sensor has at least one pose where it is at its minimum stretch, and at least one pose where it is at its maximum stretch, and that no two sensors are at the same degree of stretch in all poses. For more advanced calibration, poses with intermediate levels of stretch may be added. The key advantage to this method is the app itself does not require any prior knowledge of the locations of the sensors, and the process will inherently account for differences in the way different people wear the glove.</i>  
<br>To create your own set of reference poses:

Identify the joints in the avatar you want to control with the StretchSense Demonstration Glove and expand them all in the hierarchy of the model (screenshot of hierarchy)
Identify the co-ordinate system for each joint, i.e., rotation about what axis causes the finger to move through the desired range of motion (Note: this is avatar specific!)
Each bone is defined by 9 parameters: Translation X, Y and Z (where in the 3D volume is the center of mass of the bone relative to the workspace origin), Rotational X, Y, and Z (what orientation is the bone relative to the main axes of the workspace), and Scale X, Y and Z (the relative size of the bone in the bone's co-ordinate system. We need to set these 9 parameters for each joint in each reference pose. Note in Unity, the bones in the fingers are setup with a parent-child relationship. This emans for example that rotating a finger at the metacarpalphalangeal joint moves the proximate phalangeal joint  
Set the location, orientation and scale parameters for each joint for each reference pose you wish to include in the calibration sequence.
Record the parameters to use to drive the hand animation during the Calibration process and program each reference pose into the CalibrationAlgorithm component of the project.

DemoRun

#### How to find coordinates for poses?

In this video we will show how assets coordinates are used while calibration. An assets may have different coordinates so we need to check in unity first how x,y & z are behaving before we set them up. Show Video: ShowingPoses

#### Brief explanation on Training Positions : TrainingPositions Brief

# Demo Single Finger Test:
<ol>
<li>Lets run through a demo where we are going to only use Index finger as a reference, define some poses and see movement of index finger in realtime. Remember we are still considering data from every sensor while we calculate index finger movement. So accuracy may not be perfect but we don't have to tell system which sensor belongs to Index finger. The poses are define in a way we try to stretch index sensor from 0 to 100 and also positions we are worried in realtime to be accurate. See Video Onefingerdemo</li>
<li>Now lets run the code and see the finger movement in the demo app: SingleFingerDemoAll</li>
<li>Now lets tell the system to look for Index sensor for matching index movement and perform calculation on the same:SetIndexToIndex</li>
<li>Finally lets try to run this update demo. As you can see the accuracy improves as compared to system looking for all the sensor data for just single Index finger.IndexToLookAtIndex</li>
</ol>
