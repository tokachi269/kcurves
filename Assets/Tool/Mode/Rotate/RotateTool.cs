using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace CamOpr.Tool
{
    public class RotateTool : BaseCameraMode, ICameraMode
    {
        public override string Name { get; set; }

        //ユーザー制御点
        protected List<CameraConfig> Knots { get; set; } = new List<CameraConfig>();
        [SerializeField]
        public float Time { get; set; }
        [SerializeField]
        public float TimePerRound { get; set; } = 4f;
        [SerializeField]
        public bool IsCameraShake { get; set; }
     //   public PerlinCameraShake CameraShake;

        protected CameraConfig DefaultPosition { get; set; }
        private GameObject moveCameraCube;

        public void Start()
        {
           // CameraShake = gameObject.AddComponent<PerlinCameraShake>();
           // CameraShake.enabled = false;

            moveCameraCube = new GameObject("moveCameraCube");
            moveCameraCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            moveCameraCube.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            moveCameraCube.GetComponent<Renderer>().material.color = Color.blue;
            moveCameraCube.transform.parent = this.transform;
        }

        public IEnumerator Play()
        {
            DefaultPosition = CameraUtils.CameraPosition();

            if (IsCameraShake)
            {
         //       CameraShake.enabled = true;
            }

            if (TimePerRound == 0)
            {
                Debug.LogError("The cycle time cannot be set to zero.");
                yield break;
            }

            for (float currentTime = 0; currentTime <= Time; )
            {
                float dt = UnityEngine.Time.deltaTime;

                GameObject.Find("Main Camera").transform.RotateAround(
                    Knots[0].Position,
                    Vector3.up,
                    360 / TimePerRound * dt
                );

                currentTime += dt;
                yield return  null;
            }

            if (IsCameraShake)
            {
        //        CameraShake.enabled = false;
            }

            moveCameraCube.transform.position = DefaultPosition.Position;
            moveCameraCube.transform.rotation = DefaultPosition.Rotation;
        }

        public void AddKnot(Vector3 position, Quaternion rotation, float fov)
        {
            this.Knots.Add(new CameraConfig(position, rotation, fov));
            if (Knots.Count == 0)
            {
                moveCameraCube.transform.position = Knots[0].Position;
            }
        }

        public void RemoveKnot()
        {
            this.Knots.RemoveAt(Knots.Count - 1);
        }

        public void AddKnot(CameraConfig cp, float? param = null)
        {
            this.Knots.Add(cp);
            if (Knots.Count == 0)
            {
                moveCameraCube.transform.position = Knots[0].Position;
            }
        }


        public void Render()
        {
            throw new NotImplementedException();
        }

        public class Serializer : ISerialize
        {
            RotateTool Instance;
            public Serializer(RotateTool instance)
            {
                this.Instance = instance;
            }
            public void ToXML()
            {
                //TODO ハードコーディングをやめる
                var xml = new XDocument(
                    new XElement("Paths",
                        new XElement("Instance",
                            new XElement("Knots", Instance.Knots),
                            new XElement("Time", Instance.Time),
                            new XElement("TimePerRound", Instance.TimePerRound),
                            new XElement("IsCameraShake", Instance.IsCameraShake)
                        )
                    )
                );
                xml.Save(@"C:\Temp\LINQ_to_XML_Sample2.xml");
            }

            public void Serialize(string filename)
            {
                string text = System.IO.Path.Combine(ToolController.RecoveryDirectory, filename + ".xml");
                try
                {
                    if (!Directory.Exists(ToolController.RecoveryDirectory))
                    {
                        Directory.CreateDirectory(ToolController.RecoveryDirectory);
                    }

                    using (FileStream fileStream = new FileStream(text, FileMode.OpenOrCreate))
                    {
                        fileStream.SetLength(0L);
                        new XmlSerializer(this.GetType()).Serialize(fileStream, this);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            public void Deserialize(string filename)
            {
                //var path = System.IO.Instance.Combine(ToolController.RecoveryDirectory, filename + ".xml");
                //if (File.Exists(path))
                //{
                //    List<CameraConfig> list = new List<CameraConfig>();
                //    try
                //    {
                //        using (FileStream fileStream = new FileStream(path, FileMode.Open))
                //        {
                //            list = (new XmlSerializer(list.GetType()).Deserialize(fileStream) as List<CameraConfig>);
                //        }
                //    }
                //    catch (Exception e)
                //    {
                //        Debug.LogException(e);
                //    }
                //
                //}
            }
        }
    }
}
