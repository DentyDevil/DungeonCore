    using UnityEngine;

    public class EnemyAttackCoreState : EnemyBaseState
    {
        float timer;
        public EnemyAttackCoreState(BaseEnemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
        
        }
        public override void Enter()
        {
            timer = enemy.enemy.attackCoolDown;
        }
        public override void Execute()
        {
            AttackCore();
        }
        public override void Exit()
        {

        }

        void AttackCore()
        {
            timer += Time.deltaTime;
            if(timer < enemy.enemy.attackCoolDown) return;
            timer = 0;
            float dungeonHealth = DungeonCore.Instance.dungeonCoreHealthPoints;
            if (dungeonHealth <= 0) { Debug.LogWarning("ядро уничтожено... игра окончена!"); }
            else { DungeonCore.Instance.dungeonCoreHealthPoints -= enemy.enemy.damage; Debug.LogWarning($"ядро получило урон - {enemy.enemy.damage} от - {enemy.enemy.name} осталось хп - {dungeonHealth}"); }
        
        }
    }
