using UnityEngine;
using VContainer.Unity;

namespace PlayingCard.GamePlay.GameState
{
    public enum GameState
    {
        MainMenu,
        GameRoom
    }

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
