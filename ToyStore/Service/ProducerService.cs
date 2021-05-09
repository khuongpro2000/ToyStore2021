﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ToyStore.Data;
using ToyStore.Models;

namespace ToyStore.Service
{
    public interface IProducerService
    {
        Producer AddProducer(Producer producer);
        IEnumerable<Producer> GetProducerList();
        IEnumerable<Producer> GetProducerList(string keyWord);
        IEnumerable<Producer> GetProducerListName(string keyword);
        Producer GetByID(int ID);
        void UpdateProducer(Producer producer);
        void Block(Producer producer);
        void Active(Producer producer);
        void MultiDeleteProducer(string[] IDs);
        void Save();
    }
    public class ProducerService : IProducerService
    {
        private readonly UnitOfWork context;
        public ProducerService(UnitOfWork repositoryContext)
        {
            this.context = repositoryContext;
        }
        public Producer AddProducer(Producer producer)
        {
            producer.LastUpdatedDate = DateTime.Now;
            this.context.ProducerRepository.Insert(producer);
            return producer;
        }
        public void MultiDeleteProducer(string[] IDs)
        {
            foreach (var id in IDs)
            {
                Producer producer = GetByID(int.Parse(id));
                producer.IsActive = false;
                UpdateProducer(producer);
            }
        }

        public Producer GetByID(int ID)
        {
            return this.context.ProducerRepository.GetDataByID(ID);
        }

        public IEnumerable<Producer> GetProducerList()
        {
            IEnumerable<Producer> listProducer = this.context.ProducerRepository.GetAllData();
            return listProducer;
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void UpdateProducer(Producer producer)
        {
            producer.LastUpdatedDate = DateTime.Now;
            this.context.ProducerRepository.Update(producer);
        }

        public IEnumerable<Producer> GetProducerList(string keyWord)
        {
            IEnumerable<Producer> listProducer = this.context.ProducerRepository.GetAllData(x => x.Name.Contains(keyWord));
            return listProducer;
        }

        public IEnumerable<Producer> GetProducerListName(string keyword)
        {
            IEnumerable<Producer> listProducerName = this.context.ProducerRepository.GetAllData(x => x.Name.Contains(keyword));
            return listProducerName;
        }

        public void Block(Producer producer)
        {
            producer.IsActive = false;
            this.context.ProducerRepository.Delete(producer);
        }

        public void Active(Producer producer)
        {
            producer.IsActive = true;
            this.context.ProducerRepository.Update(producer);
        }
    }
}