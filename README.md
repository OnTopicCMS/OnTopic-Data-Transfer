# OnTopic Data Transfer Library
The `OnTopic.Data.Transfer` assembly defines a data transfer object to act as an intermediary between the `Topic` class and serialization targets (such as JSON) used for web services.

[![OnTopic.Data.Transfer package in Internal feed in Azure Artifacts](https://igniasoftware.feeds.visualstudio.com/_apis/public/Packaging/Feeds/46d5f49c-5e1e-47bb-8b14-43be6c719ba8/Packages/b26b3967-7a48-44e8-8bc4-de7624cde801/Badge)](https://igniasoftware.visualstudio.com/OnTopic/_packaging?_a=package&feed=46d5f49c-5e1e-47bb-8b14-43be6c719ba8&package=b26b3967-7a48-44e8-8bc4-de7624cde801&preferRelease=true)
[![Build Status](https://igniasoftware.visualstudio.com/OnTopic/_apis/build/status/OnTopic-Data-Transfer-CI-V3?branchName=master)](https://igniasoftware.visualstudio.com/OnTopic/_build/latest?definitionId=13&branchName=master)

## Contents
- [Motivation](#motivation)
- [Data Model](#data-model)
- [Data Interchange](#data-interchange)
  - [Export Options](#export-options)
  - [Import Options](#import-options)
- [Serialization and Deserialization](#serialization-and-deserialization)
  - [Serialization](#serialization)
  - [Deserialization](#deserialization)
- [Installation](#installation)

## Motivation
Technically, the `Topic` class—as well as its dependents—could be directly serialized. Because of the complexity of these classes, however, that would introduce a lot of clutter and complexity to the JSON format. This could be avoided, but would almost certainly require not only careful annotation of the entity classes, but quite possibly the introduction of custom converters. Further, even were this done, it wouldn't provide an opportunity to e.g. merge deserialized topics with an existing topic graph. Providing intermediary data transfer objects which offers a simplified data structure for modeling a topic, as well as extension methods for converting them to and from `Topic` classes addresses this problem.

## Data Model
The `OnTopic.Data.Transfer` assembly includes three basic data transfer classes along side a custom collection class:

- `TopicData`: Maps to the `Topic` class, and includes the following collections:
  - `Children` (`List[TopicData]`)
  - `Attributes` (`List[AttributeData]`)
    - `AttributeData`: Maps to the `AttributeValue` class, and represents an individual attribute.
  - `Relationships` ([`RelationshipDataCollection`](OnTopic.Data.Transfer/RelationshipDataCollection.cs))
    - `RelationshipData`: Maps to the `NamedTopicCollection`, and represents a relationship key, as well as a list of references to related topics via their `Topic.GetUniqueKey()` value.

## Data Interchange
The `OnTopic.Data.Transfer.Interchange` namespace includes extension methods for the `OnTopic.Topic` entity which allow a `TopicData` graph to be exported from a `Topic` graph—or imported back into one:

- `Topic.Export()`: Exports a topic, and all child topics, into a `TopicData` graph.
- `Topic.Import()`: Imports a `TopicData` graph into an existing `Topic`, by default leaving preexisting attributes and relationships alone.

### Export Options
Optionally, the `Topic.Export()` extension method will accept an [`ExportOptions`](OnTopic.Data.Transfer/Interchange/ExportOptions.cs) object as argument in order to fine-tune the business logic for the export. This includes the following options:

- `IncludeExternalReferences`: Enables relationships to be exported, even if the topics they point to fall outside the scope of the current export.
- `IncludeNestedTopics`: Includes nested topic as part of the export.
- `IncludeChildTopics`: Recursively includes _all_ child topics—including nested topics—as part of the export. Implies `IncludeNestedTopics`.

### Import Options
Optionally, the `Topic.Import()` extension method will accept an [`ImportOptions`](OnTopic.Data.Transfer/Interchange/ImportOptions.cs) object as argument in order to fine-tune the business logic for the import. Most notably, this includes a `Strategy` property for setting the [`ImportStrategy`](OnTopic.Data.Transfer/Interchange/ImportStrategy.cs) enum, which includes the following options:

- `Add`: If a `Topic` exists, will only add new attributes to the object, while leaving existing attributes alone. This is the default.
- `Merge`: If a `Topic` exists, will only add newer attributes to the object, while leaving other attributes alone.
- `Overwrite`: If a `Topic` exists, will overwrite _all_ attributes, even if the target attribute is _newer_.
- `Replace`: Will not only overwrite any matched attributes, but will additionally _delete_ any _unmatched_ topics, attributes, or relationships, thus ensuring the target object graph is identical to the source `TopicData` graph.

In addition to the `Strategy` property, the `ImportOptions` also allows the behavior to be fine-tuned, if further control is required:

- `DeleteUnmatchedAttributes`: Determines if unmatched attributes should be deleted. Defaults to `false` unless `ImportStrategy.Replace` is set.
- `DeleteUnmatchedRelationships`: Determines if unmatched relationships should be deleted. Defaults to `false` unless `ImportStrategy.Replace` is set.
- `DeleteUnmatchedChildren`: Determines if unmatched children should be deleted. Defaults to `false` unless `ImportStrategy.Replace` is set.
- `DeleteUnmatchedNestedTopics`: Determines if unmatched nested topics should be deleted. Defaults to `false` unless `ImportStrategy.Replace` is set.
- `OverwriteContentType`: Determines if the `ContentType` on an existing `Topic` should be updated if the `TopicData` value is different. Defaults to `false` unless `ImportStrategy` is set to `Ovewrite` or `Replace`.

## Serialization and Deserialization
Serializing `TopicData` graphs to JSON and deserializing JSON back to a `TopicData` graph operates exactly like it would for any other C# objects.

### Serialization
```
var topicData = topic.Export();
var json = JsonSerializer.Serialize(topicData);
```

### Deserialization
```
var topicData = JsonSerializer.Deserialize<TopicData>(json);
```

## Installation
Installation can be performed by providing a `<PackageReference /`> to the `OnTopic.Data.Transfer` **NuGet** package.
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  …
  <ItemGroup>
    <PackageReference Include="OnTopic.Data.Transfer" Version="1.0.0" />
  </ItemGroup>
</Project>
```

> *Note:* This package is currently only available on Ignia's private **NuGet** repository. For access, please contact [Ignia](http://www.ignia.com/).