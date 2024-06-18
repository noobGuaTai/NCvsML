namespace PlayerEnum
{
    public enum PlayerType
    {
        player1,
        player2
    }
    public enum PlayerStateType
    {
        Idle,
        Move,
        Jump
    }
    public enum PlayerActionType
    {
        None,
        MoveLeft,
        MoveRight,
        Jump,
        Shoot,
        StartNextGround
    }

}