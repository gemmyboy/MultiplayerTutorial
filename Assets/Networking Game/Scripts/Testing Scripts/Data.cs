using UnityEngine;
using System.Collections;
using System.Data;
using Mono.Data.SqliteClient;
public class Data : MonoBehaviour {
    private string _constr = "URI=file:NPCMaster.db";
    private IDbConnection _dbc;
    private IDbCommand _dbcm;
    private IDataReader _dbr;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
