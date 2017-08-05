﻿using Destiny.Core.IO;
using Destiny.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Destiny.Maple.Characters
{
    public sealed class CharacterSkills : KeyedCollection<int, Skill>
    {
        public Character Parent { get; private set; }

        public CharacterSkills(Character parent)
             : base()
        {
            this.Parent = parent;
        }

        public void Load()
        {
            foreach (Datum datum in new Datums("skills").Populate("CharacterID = '{0}'", this.Parent.ID))
            {
                this.Add(new Skill(datum));
            }
        }

        public void Save()
        {
            foreach (Skill skill in this)
            {
                skill.Save();
            }
        }

        public void Delete()
        {
            foreach (Skill skill in this)
            {
                skill.Delete();
            }
        }

        public void Cast(InPacket iPacket)
        {
            iPacket.ReadInt(); // NOTE: Ticks.
            int mapleID = iPacket.ReadInt();

            Skill skill = this[mapleID];
            int level = iPacket.ReadByte();

            if (level != skill.CurrentLevel)
            {
                return;
            }

            skill.Cast();
        }

        public void Encode(OutPacket oPacket)
        {
            oPacket.WriteShort((short)this.Count);

            List<Skill> cooldownSkills = new List<Skill>();

            foreach (Skill loopSkill in this)
            {
                loopSkill.Encode(oPacket);

                if (loopSkill.IsCoolingDown)
                {
                    cooldownSkills.Add(loopSkill);
                }
            }

            oPacket.WriteShort((short)cooldownSkills.Count);

            foreach(Skill loopCooldown in cooldownSkills)
            {
                oPacket
                    .WriteInt(loopCooldown.MapleID)
                    .WriteShort((short)loopCooldown.RemainingCooldownSeconds);
            }
        }

        protected override void InsertItem(int index, Skill item)
        {
            item.Parent = this;

            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            Skill item = base.Items[index];

            item.Parent = null;

            base.RemoveItem(index);
        }

        protected override int GetKeyForItem(Skill item)
        {
            return item.MapleID;
        }
    }
}

