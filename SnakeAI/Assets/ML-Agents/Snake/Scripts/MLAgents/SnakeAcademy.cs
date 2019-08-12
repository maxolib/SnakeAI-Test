using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class SnakeAcademy : Academy {
    public int StateSize;
    public float Speed;
    public override void InitializeAcademy()
    {
        StateSize = (int) resetParameters["StateSize"];
        Speed = resetParameters["Speed"];
    }
    public override void AcademyReset()
    {

    }

    public override void AcademyStep()
    {


    }

}
