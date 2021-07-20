import UnityEngine

class SQUADTesting (MonoBehaviour): 
	public alongLine as single
	public parentTransform as Transform
	public speed as single

	def Start ():
		pass
	
	def Update ():
		quaternionSplinePoints = array(Quaternion,parentTransform.childCount)
		for i in range(parentTransform.childCount):
			quaternionSplinePoints[i] = parentTransform.GetChild(i).rotation
		
		transform.rotation = SQUAD.Spline(quaternionSplinePoints, alongLine)
		alongLine += Time.deltaTime * speed
		if alongLine >= 1:
			alongLine = 0
			