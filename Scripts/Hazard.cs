using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Hazard : MonoBehaviour
{
   /// <summary>
   /// How much damage the hazard applies to the player every second
   /// </summary>
   public float damageOverTime = 1;

   /// <summary>
   /// flat damage on first entry
   /// </summary>
   public float damageOnEntry = 10;
   
   //public float damageIncreaseOverTime = 0.5f;
   //private float timeSincePlayerEntered = 0;
   //private bool playerEntered = false;

   Collider col;
   // Start is called before the first frame update
   void Start()
   {
      col = GetComponent<Collider>();
      col.isTrigger = true;
   }
   private void OnTriggerEnter(Collider other)
   {
      if (other.tag == "Marshmellow")
      {
         Marshmellow mellow = other.GetComponent<Marshmellow>();
         if (mellow != null)
         {
            mellow.health -= damageOnEntry;
         }
      }
   }
   private void OnTriggerStay(Collider other)
   {
      if (other.tag == "Marshmellow")
      {
         Marshmellow mellow = other.GetComponent<Marshmellow>();
         if (mellow != null)
         {
            mellow.health -= damageOverTime * Time.deltaTime;
         }
      }
   }
   
}
