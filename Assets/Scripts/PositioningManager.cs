using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PositioningManager : MonoBehaviour
{
    [System.Serializable]
    public class Position
    {
        //포지셔닝 데이터 하나(1축,2축,3축)를 들고 있음.
        public int axis1;
        public int axis2;
        public int axis3;
        //연결되어 있는 UI
        private PositionData uiData;

        public PositionData GetData => uiData;

        //생성자
        public Position(int axis1, int axis2, int axis3)
        {
            this.axis1 = axis1;
            this.axis2 = axis2;
            this.axis3 = axis3;
        }

        //데이터와 연계된 UI를 연결하는 함수
        public void ConnectUI(PositionData data)
        {
            uiData = data;
        }
    }

    //UI원본
    public PositionData origin;

    //포지셔닝 데이터를 모아두는 리스트
    public List<Position> positionList = new List<Position>();
    public ServoAmp axis1;
    public ServoAmp axis2;
    public ServoAmp axis3;

    //리스트에 원하는 위치결정값을 추가하는 함수
    public void AddData()
    {
        AddData(axis1.GetCurrentPulse, axis2.GetCurrentPulse, axis3.GetCurrentPulse, true);
    }
    public void AddData(int axis1, int axis2, int axis3, bool needSave)
    {
        Position pos = new Position(axis1, axis2, axis3);
        positionList.Add(pos);
        PositionData uiData = Instantiate(origin, transform);
        uiData.Initialize(positionList.Count, axis1, axis2, axis3);
        uiData.gameObject.SetActive(true);
        pos.ConnectUI(uiData);

        if (needSave)
            SaveData();
    }

    //들어있는 위치결정값을 제거하는 함수
    public void RemoveData(PositionData uiData)
    {
        //삭제하고 싶은 UI와 연결될 위치결정 데이터를 찾아낸다.
        Position pos = positionList.Find(x => x.GetData == uiData);
        //데이터를 찾았다면 지운다.
        if(pos != null)
        {
            positionList.Remove(pos);
        }

        //순서가 바뀐 리스트에 맞게 UI텍스트를 수정한다.
        for(int i = 0; i < positionList.Count; ++i)
        {
            positionList[i].GetData.ChangeIndex(i);
        }
    }

    //파일로부터 위치결정 데이터들을 불러오는 함수
    public void LoadData()
    {
        string path = Path.Combine(Application.dataPath, "PositionData.csv");
        if (!File.Exists(path))
        {
            Debug.LogError("파일이 존재하지 않아 불러올 수 없습니다!!");
            return;
        }

        string[] csvDatas = File.ReadAllLines(path);
        //2줄 이상의 데이터가 들어있을때만 데이터를 가져올 예정
        if(csvDatas.Length > 1)
        {
            foreach(var position in positionList)
            {
                position.GetData.Delete(false);
            }
            //새로운 리스트를 만들어서 로드함.
            positionList = new List<Position>();

            //파일 기준으로 다시 데이터를 생성해서 리스트를 채워줌
            for (int i = 1; i < csvDatas.Length; ++i)
            {
                //','를 기준으로 문자열을 잘라내서 배열로 모아둠
                string[] datas = csvDatas[i].Split(',');
                AddData(int.Parse(datas[1]), int.Parse(datas[2]), int.Parse(datas[3]), false);
            }
        }
        else
        {
            Debug.LogWarning("파일 안에 데이터가 들어있지 않아 불러오기를 취소합니다!!");
        }

    }





    //위치결정 데이터들을 파일에 저장하는 함수
    public void SaveData()
    {
        string path = Path.Combine(Application.dataPath, "PositionData.csv");

        string[] csvDatas = new string[positionList.Count + 1];
        csvDatas[0] = "Position ID, Axis1, Axis2, Axis3";
        for (int i = 0; i < positionList.Count; ++i)
        {
            csvDatas[i + 1] = i.ToString() + ',' +
                positionList[i].axis1.ToString() + ',' +
                positionList[i].axis2.ToString() + ',' +
                positionList[i].axis3.ToString();
        }

        //해당 경로에 파일로 저장하기.
        File.WriteAllLines(path, csvDatas);
    }

    private void Start()
    {
        LoadData();
    }

}
