using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Anodyne {
    public class HealthBar : MonoBehaviour {

        [System.NonSerialized]
        public int MaxHealth = 6;

        [Range(0,24)]
        public int TESTCurrentHealth = 6;
        [Range(0,6)]
        public int TESTHealthUpgradesEarned = 6;
        public bool TESTDEBUG = false;

        RectTransform BarBackgroundT;
        RectTransform RedBarT;
        RectTransform GoldBarT;

        float BarYPositionAtInvisible = -107.5f;
        float BackgroundYPositionAtMinimumMaxHealth = -48.5f;
        float BarOneUnitIncrement = 8f;

        [System.NonSerialized]
        public string nameAddendum = "";

        bool sf = false;
        private void Start() {
            if (name == "HealthBarMaskpm") {
                nameAddendum = "pm";
                BarYPositionAtInvisible = -107f;
                BackgroundYPositionAtMinimumMaxHealth = -48f;
            }
            BarBackgroundT = GameObject.Find(nameAddendum+"HealthBar_Empty").GetComponent<RectTransform>();
            RedBarT = GameObject.Find(nameAddendum + "HealthBar_Red").GetComponent<RectTransform>();
            GoldBarT= GameObject.Find(nameAddendum + "HealthBar_Gold").GetComponent<RectTransform>();
            // Only the 2D Health bar actually heals on scene entry/ is allowed to modify current health
            
            if (name != "HealthBarMaskpm") {
                // Do this bc a recent load causes currentHealth to have some other value, which we want to use instead of just healing
                int old = SaveManager.currentHealth;
                UpdateMaxHealthAndFullHeal();
                SetHealth(old);
            }
            sf = true;
        }

        Vector3 pos = new Vector3();

        private void OnValidate() {
            if (!Application.isPlaying) return;
            if (!sf) return;
            if (TESTDEBUG) {
                SaveManager.currentHealth = TESTCurrentHealth;
                SaveManager.healthUpgrades = TESTHealthUpgradesEarned;
            }
            UpdateMaxHealthAndFullHeal();
            FullHeal();
        }

        public void SetHealth(int newhealth) {
            UpdateMaxHealthAndFullHeal();
            if (newhealth > MaxHealth) newhealth = MaxHealth;
            SaveManager.currentHealth = newhealth;
            
            UpdateGraphics();
        }
        

        public void UpdateMaxHealthAndFullHeal() {
            MaxHealth = 6 + SaveManager.healthUpgrades;
            if (SaveManager.doubleHealth) MaxHealth *= 2;
            if (SaveManager.currentHealth < MaxHealth) SaveManager.currentHealth = MaxHealth;
            UpdateGraphics();
        }

        void UpdateGraphics() {
            pos = BarBackgroundT.localPosition;
            pos.y = BackgroundYPositionAtMinimumMaxHealth + BarOneUnitIncrement * SaveManager.healthUpgrades;
            BarBackgroundT.localPosition = pos;

            if (SaveManager.doubleHealth) {
                pos = GoldBarT.localPosition;
                if (SaveManager.currentHealth > MaxHealth / 2) {
                    pos.y = BarYPositionAtInvisible + BarOneUnitIncrement * (SaveManager.currentHealth - (MaxHealth/2));
                } else {
                    pos.y = BarYPositionAtInvisible;
                }
                GoldBarT.localPosition = pos;

                pos = RedBarT.localPosition;
                if (SaveManager.currentHealth > MaxHealth/2) {
                    pos.y = BarYPositionAtInvisible + BarOneUnitIncrement * (MaxHealth / 2);
                } else {
                    pos.y = BarYPositionAtInvisible + BarOneUnitIncrement * SaveManager.currentHealth;
                }
                RedBarT.localPosition = pos;
            } else {
                pos = RedBarT.localPosition;
                pos.y = BarYPositionAtInvisible + BarOneUnitIncrement * SaveManager.currentHealth;
                RedBarT.localPosition = pos;
            }

        }

        public bool IsDead() {
            return (SaveManager.currentHealth == 0);
        }

        public void Damage(int amount=1) {
            SaveManager.currentHealth = Mathf.Clamp(SaveManager.currentHealth - amount, 0, MaxHealth);
            UpdateGraphics();
        }

        public void Heal(int amount=1) {
            SaveManager.currentHealth = Mathf.Clamp(SaveManager.currentHealth + amount, 0, MaxHealth);
            UpdateGraphics();
        }
        public void FullHeal() {
            SaveManager.currentHealth = MaxHealth;
            UpdateGraphics();
        }
        public void InstantKill() {
            SaveManager.currentHealth = 0;
            UpdateGraphics();
        }

    }
}