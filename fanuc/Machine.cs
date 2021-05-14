﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using fanuc.collectors;
using fanuc.handlers;
using fanuc.veneers;

namespace fanuc
{
    public class Machine
    {
        public override string ToString()
        {
            return new
            {
                Id,
                _focasEndpoint.IPAddress,
                _focasEndpoint.Port,
                _focasEndpoint.ConnectionTimeout
            }.ToString();
        }

        public dynamic Info
        {
            get
            {
                return new
                {
                    Id,
                    _focasEndpoint.IPAddress,
                    _focasEndpoint.Port,
                    _focasEndpoint.ConnectionTimeout
                };
            }
        }
        
        public FocasEndpoint FocasEndpoint
        {
            get { return _focasEndpoint; }
        }
        
        private FocasEndpoint _focasEndpoint;
        
        public Platform Platform
        {
            get { return _platform; }
        }
        
        private Platform _platform;
        
        public Veneers Veneers
        {
            get { return _veneers; }
        }
        
        private Veneers _veneers;

        public Handler Handler
        {
            get { return _handler; }
        }
        
        private Handler _handler;
        
        public Collector Collector
        {
            get { return _collector; }
        }
        
        private Collector _collector;
        
        public bool Enabled
        {
            get { return _enabled; }
        }
        
        private bool _enabled = false;
        
        public string Id
        {
            get { return _id; }
        }
        
        private string _id = string.Empty;
        
        public bool CollectorSuccess
        {
            get { return _collector.LastSuccess;  }
        }
        
        public Machine(bool enabled, string id, string focasIpAddress, ushort focasPort = 8193, short timeout = 10)
        {
            _enabled = enabled;
            _id = id;
            _focasEndpoint = new FocasEndpoint(focasIpAddress, focasPort, timeout);
            _platform = new Platform(this);
            _veneers = new Veneers(this);
        }

        public void AddHandler(Type type, dynamic config)
        {
            _handler = (Handler) Activator.CreateInstance(type, new object[] { this });
            _handler.Initialize(config);
            _veneers.OnDataArrival = _handler.OnDataArrivalInternal;
            _veneers.OnDataChange = _handler.OnDataChangeInternal;
            _veneers.OnError = _handler.OnErrorInternal;
        }
        
        public void AddCollector(Type type, int sweepMs = 1000)
        {
            _collector = (Collector) Activator.CreateInstance(type, new object[] { this, sweepMs });
        }

        public void InitCollector()
        {
            _collector.Initialize();
        }

        public void RunCollector()
        {
            _collector.Collect();
        }
        
        public bool VeneersApplied { get; set; }

        public void ApplyVeneer(Type type, string name, bool isInternal = false)
        {
            _veneers.Add(type, name, isInternal);
        }

        public void SliceVeneer(dynamic split)
        {
            _veneers.Slice(split);
        }
        
        public void SliceVeneer(dynamic sliceKey, dynamic split)
        {
            _veneers.Slice(sliceKey, split);
        }

        public void ApplyVeneerAcrossSlices(Type type, string name, bool isInternal = false)
        {
            _veneers.AddAcrossSlices(type, name, isInternal);
        }
        
        public void ApplyVeneerAcrossSlices(dynamic sliceKey, Type type, string name, bool isInternal = false)
        {
            _veneers.AddAcrossSlices(sliceKey, type, name, isInternal);
        }

        public dynamic PeelVeneer(string name, dynamic input, dynamic? input2 = null)
        {
            return _veneers.Peel(name, input, input2);
        }
        
        public dynamic PeelAcrossVeneer(dynamic split, string name, dynamic input, dynamic? input2 = null)
        {
            return _veneers.PeelAcross(split, name, input, input2);
        }

        public void MarkVeneer(dynamic split, dynamic marker)
        {
            _veneers.Mark(split, marker);
        }
    }
}