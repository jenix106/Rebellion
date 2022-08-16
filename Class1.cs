using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Rebellion
{
    public class RebellionModule : ItemModule
    {
        public float DashSpeed;
        public string DashDirection;
        public bool DisableGravity;
        public bool DisableCollision;
        public string ActivationButton = "Alt Use";
        public float DashTime;
        public bool StopOnEnd = false;
        public bool StopOnStart = false;
        public bool ThumbstickDash = false;
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<RebellionComponent>().Setup(DashSpeed, DashDirection, DisableGravity, DisableCollision, DashTime, ActivationButton, StopOnEnd, StopOnStart, ThumbstickDash);
        }
    }
    public class RebellionComponent : MonoBehaviour
    {
        Item item;
        bool dashing;
        public float DashSpeed;
        public string DashDirection;
        public bool DisableGravity;
        public bool DisableCollision;
        public float DashTime;
        public bool StopOnEnd;
        public bool StopOnStart;
        bool ThumbstickDash;
        bool fallDamage;
        Interactable.Action ActivationButton;
        public void Start()
        {
            item = GetComponent<Item>();
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
        }

        public void Setup(float speed, string direction, bool gravity, bool collision, float time, string button, bool stop, bool start, bool thumbstick)
        {
            DashSpeed = speed;
            DashDirection = direction;
            DisableGravity = gravity;
            DisableCollision = collision;
            DashTime = time;
            if (button.ToLower().Contains("trigger") || button.ToLower() == "use")
            {
                ActivationButton = Interactable.Action.UseStart;
            }
            else if (button.ToLower().Contains("alt") || button.ToLower().Contains("spell"))
            {
                ActivationButton = Interactable.Action.AlternateUseStart;
            }
            if (direction.ToLower().Contains("player") || direction.ToLower().Contains("head") || direction.ToLower().Contains("sight"))
            {
                DashDirection = "Player";
            }
            else if (direction.ToLower().Contains("item") || direction.ToLower().Contains("sheath") || direction.ToLower().Contains("flyref") || direction.ToLower().Contains("weapon"))
            {
                DashDirection = "Item";
            }
            StopOnEnd = stop;
            StopOnStart = start;
            ThumbstickDash = thumbstick;
        }
        public void FixedUpdate()
        {
            if (!dashing) fallDamage = Player.fallDamage;
        }
        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (action == ActivationButton)
            {
                StopCoroutine(Dash());
                StartCoroutine(Dash());
            }
        }
        public IEnumerator Dash()
        {
            dashing = true;
            Player.fallDamage = false;
            if (StopOnStart) Player.local.locomotion.rb.velocity = Vector3.zero;
            if (Player.local.locomotion.moveDirection.magnitude <= 0 || !ThumbstickDash)
                if (DashDirection == "Item")
                {
                    Player.local.locomotion.rb.AddForce(item.holderPoint.forward * DashSpeed, ForceMode.Impulse);
                }
                else
                {
                    Player.local.locomotion.rb.AddForce(Player.local.head.transform.forward * DashSpeed, ForceMode.Impulse);
                }
            else
            {
                Player.local.locomotion.rb.AddForce(Player.local.locomotion.moveDirection.normalized * DashSpeed, ForceMode.Impulse);
            }
            if (DisableGravity)
                Player.local.locomotion.rb.useGravity = false;
            if (DisableCollision)
            {
                Player.local.locomotion.rb.detectCollisions = false;
                item.rb.detectCollisions = false;
                item.mainHandler.rb.detectCollisions = false;
                item.mainHandler.otherHand.rb.detectCollisions = false;
            }
            yield return new WaitForSeconds(DashTime);
            if (DisableGravity)
                Player.local.locomotion.rb.useGravity = true;
            if (DisableCollision)
            {
                Player.local.locomotion.rb.detectCollisions = true;
                item.rb.detectCollisions = true;
                item.mainHandler.rb.detectCollisions = true;
                item.mainHandler.otherHand.rb.detectCollisions = true;
            }
            if (StopOnEnd) Player.local.locomotion.rb.velocity = Vector3.zero;
            Player.fallDamage = fallDamage;
            dashing = false;
            yield break;
        }
    }
}
