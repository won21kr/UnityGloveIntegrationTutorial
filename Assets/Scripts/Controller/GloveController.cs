using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GloveController : MonoBehaviour {

	
	public GameObject Thumb;
	public GameObject Index;
	public GameObject Middle;
	public GameObject Ring;
	public GameObject Pinky;

	public List<GameObject> ThumbChild;
	public List<GameObject> IndexChild;
	public List<GameObject> MiddleChild;
	public List<GameObject> RingChild;
	public List<GameObject> PinkyChild;

	private GloveDataProcessor gloveDataProcessor = null;

	public GameObject mainController;
	
	Boolean moveThis = false;
	private int lastTouch = 0;
	public Button btnStartCalibration;
	public Boolean eventAdded = false;

	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
		double[] Position = null;
		if (mainController != null)
		{
			gloveDataProcessor = mainController.GetComponent<GloveConnectionManager>().getGloveDataProcessor();

			if (btnStartCalibration != null && !eventAdded)
			{
				btnStartCalibration.onClick.AddListener(startCalibration);
				eventAdded = true;
			}
		}
		if (gloveDataProcessor != null)
		{
			ManageTouchEvent(gloveDataProcessor);
			Position = GetPositionData(gloveDataProcessor);

			if (Position != null && Position.Length > 0)
			{
				UpdateAvatar(Position);
			}
		}
	}
	
	
	//HERE ARE THE TRANING POSITIONS ARE PASSED INTO AND ONCE CALIBRATION IS DONE WE PASS THE REAL DATA
	
	//Set joint angles
    void UpdateAvatar(double[] Position)
    {
//	    Thumb.transform.localEulerAngles = new Vector3((float)Position[0], (float)Position[1], (float)Position[2]);
//            
//	    ThumbChild[0].transform.localEulerAngles = new Vector3(0, 0, (float)Position[3] / 2);
//	    ThumbChild[1].transform.localEulerAngles = new Vector3(0, 0, (float)Position[3]);

	    Index.transform.localEulerAngles = new Vector3((float)Position[0], 0,0);
	    IndexChild[0].transform.localEulerAngles = new Vector3((float)Position[1], 0, 0 );
	    IndexChild[1].transform.localEulerAngles = new Vector3((float)Position[2], 0, 0);

//	    Middle.transform.localEulerAngles = new Vector3((float)Position[5], 0, 0);
//	    MiddleChild[0].transform.localEulerAngles = new Vector3((float)Position[5], 0, 0);
//	    MiddleChild[1].transform.localEulerAngles = new Vector3((float)Position[5], 0, 0);
//
//	    Ring.transform.localEulerAngles = new Vector3((float)Position[6], 0, 0);
//	    RingChild[0].transform.localEulerAngles = new Vector3((float)Position[6], 0, 0);
//	    RingChild[1].transform.localEulerAngles = new Vector3((float)Position[6], 0, 0);
//
//	    Pinky.transform.localEulerAngles = new Vector3((float)Position[7], 0, 0);
//	    PinkyChild[0].transform.localEulerAngles = new Vector3((float)Position[7], 0, 0);
//	    PinkyChild[1].transform.localEulerAngles = new Vector3((float)Position[7], 0, 0);
    }

	
	//Update UI based on touch feedback
	void ManageTouchEvent(GloveDataProcessor gloveDataProcessor)
	{

		if (gloveDataProcessor.IsCalibrating)
		{
			UpdateCalibration(gloveDataProcessor);
		}
		else
		{
			RotateCharacter();
		}
	}

	
	//Get Calibration or Calibrated positions for avatar
	double[] GetPositionData(GloveDataProcessor gloveDataProcessor)
	{
		if (gloveDataProcessor.IsCalibrating) //IF IS CALIBRATION GET TRAINING POSITIONS WHICH ARE DECLARED IN CALIBRATIONALGORITHM
		{
			return gloveDataProcessor.getCurrentTrainingPosition();
		}
		else //Running
		{
			return gloveDataProcessor.getCalibratedData(); //ONCE CALIBRATION IS DONE GET REALDATA
		}
	}
	
	
	//Increment through calibration positions
	void UpdateCalibration(GloveDataProcessor gloveDataProcessor)
	{
		if (Input.touchCount > 0 && lastTouch == 0)
		{
			gloveDataProcessor.captureDataAgainstTrainingPosition();
		}
		lastTouch = Input.touchCount;
	}



	//Rotate character on screen
	void RotateCharacter()
	{

		//Determine where the touch starts (Only apply later movement to that hand)
		if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && Input.GetTouch(0).position.x > Screen.width / 2) {
			//Hand is right hand and touch started on Right side of screen
			moveThis = true;
		}
		else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && Input.GetTouch(0).position.x < Screen.width / 2)
		{
			//Hand is left hand and touch started on Left side of the screen
			moveThis = true;
		}

		//Rotate selected hand
		if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
		{
			// Get movement of the finger since last frame
			Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;

			// Move object across XY plane
			transform.Rotate(0, 0, touchDeltaPosition.x * 0.1f);
		}

		if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) {
			moveThis = false;
		}
	}

	void startCalibration()
	{
			gloveDataProcessor.startCalibration();
	}
}
