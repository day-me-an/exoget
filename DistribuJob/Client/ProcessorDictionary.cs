using System;
using System.Collections.Generic;

namespace DistribuJob.Client
{
    public class ProcessorDictionary
    {
        private readonly Dictionary<Type, List<Processor>> processorGroups = new Dictionary<Type, List<Processor>>();

        public List<Processor> this[Type type]
        {
            get
            {
                if (!type.IsSubclassOf(typeof(Processor)))
                    throw new ArgumentException("Type must be a subclass of Processor");

                List<Processor> processors;

                return processorGroups.TryGetValue(type, out processors) ? processors : null;
            }
        }

        public void Add(Processor processor)
        {
            Type processorType = processor.GetType();

            if (!processorGroups.ContainsKey(processorType))
                processorGroups[processorType] = new List<Processor>();

            processorGroups[processorType].Add(processor);
        }
    }
}
