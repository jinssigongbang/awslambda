using System;
using System.Text;
using UnityEngine;
using Amazon.Lambda;
using Amazon.Runtime;
using Amazon.CognitoIdentity;
using Amazon;
using ASWPacket;

public class CAWSLambdaManager : MonoBehaviour
{
    #region Instance
    static CAWSLambdaManager m_Instance;
    public static CAWSLambdaManager I
    {
      get
      {
        //m_Instance??= GameObject.FindObjectOfType<CAWSLambdaManager>() ;
        if (Instance == null)
        {
          Instance = new GameObject("AwsLambdaManager").AddComponent<CAWSLambdaManager>();
          Instance.Init();
        }
        return Instance;
      }
    }
    #endregion
    public string IdentityPoolId = "ap-northeast-2:271366c2-ed14-4c50-89e0-9153d3cacb2e";
    public string FunctionName = "AWSPacket";
    public string CognitoIdentityRegion = RegionEndpoint.APNortheast2.SystemName;
    public string LambdaRegion = RegionEndpoint.APNortheast2.SystemName;
    private IAmazonLambda _lambdaClient;
    private AWSCredentials _credentials;
    private AWSCredentials Credentials
    {
      get => _credentials ??= new CognitoAWSCredentials(IdentityPoolId, _CognitoIdentityRegion);
    }

    private IAmazonLambda Client
    {
      get => _lambdaClient ??= new AmazonLambdaClient(Credentials, _LambdaRegion);
    }

    private RegionEndpoint _CognitoIdentityRegion
    {
      get { return RegionEndpoint.GetBySystemName(CognitoIdentityRegion); }
    }

    private RegionEndpoint _LambdaRegion
    {
      get { return RegionEndpoint.GetBySystemName(LambdaRegion); }
    }

    public static CAWSLambdaManager Instance { get => Instance1; set => Instance1 = value; }
    public static CAWSLambdaManager Instance1 { get => m_Instance; set => m_Instance = value; }

    public void Init()
    {
      UnityInitializer.AttachToGameObject(this.gameObject);
    }
    public void SendData(CAWSPacketWriter packet, Action<CAWSPacketReader> receiveCallback, Action<string> errorCallback)
    {
      var str = JsonUtility.ToJson(packet) ;
      Debug.Log(str) ;
      Client.InvokeAsync(new Amazon.Lambda.Model.InvokeRequest()
      {
        FunctionName = FunctionName,
        Payload = JsonUtility.ToJson(packet)
      },
      (responseObject) =>
      {
        if (responseObject.Exception == null)
        {
          var fromServer = JsonUtility.FromJson<CAWSPacketReader>(Encoding.ASCII.GetString(responseObject.Response.Payload.ToArray()));
          receiveCallback?.Invoke(fromServer);
        }
        else
        {
          errorCallback?.Invoke(responseObject.Exception.ToString());
        }
      });
    }
    private void Awake()
    {
      DontDestroyOnLoad(this.gameObject);
    }
}
