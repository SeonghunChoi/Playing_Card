using UnityEngine;
using VContainer.Unity;

namespace PlayingCard.GamePlay.Presenter.Gamestate
{
    public enum GameState
    {
        MainMenu,
        Lobby,
        GameRoom
    }

    /// <summary>
    /// 각 Scene 별 상태에 따른 LifetimeScope를 지정
    /// </summary>
    public abstract class GameStateBehaviour : LifetimeScope
    {
        public virtual bool Persists
        {
            get { return false; }
        }

        public abstract GameState ActiveState { get; }

        private static GameObject goActvieStateBehaviour;

        protected override void Awake()
        {
            base.Awake();

            if (Parent != null)
            {
                Parent.Container.Inject(this);
            }
        }

        protected virtual void Start()
        {
            if (goActvieStateBehaviour != null)
            {
                if (goActvieStateBehaviour == gameObject)
                {
                    return;
                }

                var previousStateBehaviour = goActvieStateBehaviour.GetComponent<GameStateBehaviour>();
                if (previousStateBehaviour.Persists && previousStateBehaviour.ActiveState == ActiveState)
                {
                    Destroy(gameObject);
                    return;
                } 

                Destroy(goActvieStateBehaviour);
            }

            goActvieStateBehaviour = gameObject;
            if (Persists)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        protected override void OnDestroy()
        {
            if (!Persists)
            {
                goActvieStateBehaviour = null;
            }
        }
    }
}
