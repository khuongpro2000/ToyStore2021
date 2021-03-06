﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ToyStore.Data;
using ToyStore.Models;

namespace ToyStore.Service
{
    public interface IMemberService
    {
        Member AddMember(Member member);
        IEnumerable<Member> GetMemberList();
        Member CheckCapcha(int ID, string capcha);
        void UpdateCapcha(int ID, string capcha);
        Member CheckLogin(string username, string password);
        Member GetByID(int ID);
        int GetTotalMember();
        void UpdateMember(Member member);
        void DeleteMember(Member member);
        void UpdateAmountPurchased(int ID, decimal AmountPurchased);
        IEnumerable<Member> GetMemberListForStatistic();
        bool CheckEmail(string Email);
        bool CheckUsername(string Username);
        bool CheckPhoneNumber(string PhoneNumber);
        void ResetPassword(int MemberID, string NewPassword);
        void Save();
    }
    public class MemberService : IMemberService
    {
        private readonly UnitOfWork context;
        public MemberService(UnitOfWork repositoryContext)
        {
            this.context = repositoryContext;
        }

        public Member AddMember(Member member)
        {
            member.MemberTypeID = 1;
            member.AmountPurchased = 0;
            member.IsDeleted = false;
            this.context.MemberRepository.Insert(member);
            return member;
        }

        public Member CheckCapcha(int ID, string capcha)
        {
            Member member = GetByID(ID);
            if (member.Capcha == capcha)
            {
                member.EmailConfirmed = true;
                UpdateMember(member);
                return member;
            }
            return null;
        }

        public bool CheckEmail(string Email)
        {
            var check = context.MemberRepository.GetAllData(x => x.Email == Email && x.IsDeleted == false);
            if (check.Count() > 0)
            {
                return false;
            }
            return true;
        }

        public Member CheckLogin(string username, string password)
        {
            Member member = this.context.MemberRepository.GetAllData().SingleOrDefault(x => x.Username == username && x.Password == password && x.EmailConfirmed == true && x.IsDeleted == false);
            if (member == null)
            {
                member = this.context.MemberRepository.GetAllData().SingleOrDefault(x => x.Email == username && x.Password == password && x.EmailConfirmed == true && x.IsDeleted == false);
            }
            return member;
        }

        public bool CheckPhoneNumber(string PhoneNumber)
        {
            var check = context.MemberRepository.GetAllData(x => x.PhoneNumber == PhoneNumber && x.IsDeleted == false);
            if (check.Count() > 0)
            {
                return false;
            }
            return true;
        }

        public bool CheckUsername(string Username)
        {
            var check = context.MemberRepository.GetAllData(x => x.Username == Username && x.IsDeleted == false);
            if (check.Count() > 0)
            {
                return false;
            }
            return true;
        }

        public void DeleteMember(Member member)
        {
            member.IsDeleted = true;
            this.context.MemberRepository.Update(member);
        }

        public Member GetByID(int ID)
        {
            return this.context.MemberRepository.GetDataByID(ID);
        }

        public IEnumerable<Member> GetMemberList()
        {
            IEnumerable<Member> listMember = this.context.MemberRepository.GetAllData();
            return listMember;
        }

        public IEnumerable<Member> GetMemberListForStatistic()
        {
            IEnumerable<Member> listMember = this.context.MemberRepository.GetAllData(x => x.AmountPurchased > 0 && x.IsDeleted == false).OrderByDescending(x => x.AmountPurchased);
            return listMember;
        }

        public int GetTotalMember()
        {
            return context.MemberRepository.GetAllData().Count();
        }

        public void ResetPassword(int MemberID, string NewPassword)
        {
            Member member = GetByID(MemberID);
            member.Password = NewPassword;
            context.MemberRepository.Update(member);
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void UpdateAmountPurchased(int ID, decimal AmountPurchased)
        {
            Member member = context.MemberRepository.GetDataByID(ID);
            member.AmountPurchased += AmountPurchased;
            context.MemberRepository.Update(member);
        }

        public void UpdateCapcha(int ID, string capcha)
        {
            Member member = GetByID(ID);
            member.Capcha = capcha;
            UpdateMember(member);
        }

        public void UpdateMember(Member member)
        {
            this.context.MemberRepository.Update(member);
        }
    }
}