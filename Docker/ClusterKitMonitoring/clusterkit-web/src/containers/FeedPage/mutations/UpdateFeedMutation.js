import Relay from 'react-relay'

export default class UpdateFeedMutation extends Relay.Mutation {
  static fragments = {
    configuration: () => Relay.QL`
      fragment on IClusterKitNodeApi_ReleaseConfiguration {
        nodeTemplates {
          edges {
            node {
              id
              code
              configuration
              containerTypes
              minimumRequiredInstances
              maximumNeededInstances
              name
              packageRequirements {
                edges {
                  node {
                    __id
                    specificVersion
                  }
                }
              }
              priority
            }
          }
        }
        migratorTemplates {
          edges {
            node {
              id
              code
              configuration
              name
              notes
              packageRequirements {
                edges {
                  node {
                    __id
                    specificVersion
                  }
                }
              }
              priority
            }
          }
        }
        nugetFeed
        packages {
          edges {
            node {
              version
              id
              __id
            }
          }
        }
        seedAddresses
        id
      }
    `,
  };

  getMutation () {
    return Relay.QL`mutation{clusterKitNodeApi_clusterKitNodesApi_releases_update}`
  }

  getFatQuery () {
    return Relay.QL`
      fragment on ClusterKitNodeApi_Release_NodeMutationPayload {
        node
        edge
        errors {
          edges {
            node {
              field
              message
            }
          }
        }
        api {
          clusterKitNodesApi {
            releases
          }
        }
      }
    `
  }

  getConfigs () {
    return [{
      type: 'REQUIRED_CHILDREN',
      children: [
        Relay.QL`
          fragment on ClusterKitNodeApi_Release_NodeMutationPayload {
            errors {
              edges {
                node {
                  field
                  message
                }
              }
            }
            node
          }
        `,
      ],
    }]
  }

  /**
   * Convert edges list to an array of nodes; cleans unnecessary properties
   * @param edges {Object} Edges list
   * @param type {string} Converted object type
   * @returns {Object[]} Array of nodes
   */
  convertEdgesToArray(edges, type){
    const oldNodes = edges.map(x => x.node);
    const typeSingular = type.substring(0, type.length - 1);

    let nodes = [];
    oldNodes.forEach(node => {
      const keys = Object.keys(node);
      let newNode = {};
      keys.forEach(key => {
        if (key !== '__id' && key !== '__dataID__' && key !== 'id' && key !== 'packagesToInstall'){
          if (typeof(node[key]) === 'object' && node[key] && node[key].edges) {
            newNode[key] = this.convertEdgesToArray(node[key].edges, key);
          } else {
            newNode[key] = node[key];
          }
        }

        if (type === 'packages' && key === '__id') {
          newNode['id'] = node[key];
        }

        if (type === 'packageRequirements' && key === '__id') {
          newNode['id'] = node[key];
        }
      });

      // Updating a record
      if (this.props[typeSingular] && this.props[`${typeSingular}Id`] === node.id) {
        newNode = this.props[typeSingular];
      }

      // Delete a record
      if (this.props[typeSingular] && this.props[`${typeSingular}DeleteId`] === node.id) {
        newNode = null
      }

      if (this.props['migratorTemplateDeleteId'] === node.id) {
        console.log('trying to delete ', this.props['migratorTemplateDeleteId']);
      }

      if (newNode) {
        nodes.push(newNode);
      }
    });

    // Adding a record
    if (this.props[typeSingular]) {
      console.log('Creating or updating', this.props[`${typeSingular}Id`]);
    }
    if (this.props[typeSingular] && this.props[`${typeSingular}Id`] === null) {
      nodes.push(this.props[typeSingular]);
    }

    // seedAddresses are passed as an simple string array, without id's or anything
    if (this.props[typeSingular] === 'seedAddresses') {
      nodes = this.props[typeSingular];
    }

    return nodes;
  }

  /**
   * Checks if seed addresses are being updated and return old / updated list as needed
   * @param seedAddresses {string[]} Seed addresses list
   * @return {string[]} Updated seed addresses list
   */
  updateSeedAddresses(seedAddresses) {
    console.log('old seedAddresses', seedAddresses);
    console.log('new seedAddresses', this.props.seedAddresses);
    if (this.props.seedAddresses) {
      return this.props.seedAddresses;
    }

    return seedAddresses;
  }

  getVariables () {
    return {
      id: this.props.releaseId,
      newNode: {
        id: this.props.releaseId,
        configuration: {
          nodeTemplates: this.convertEdgesToArray(this.props.configuration.nodeTemplates.edges, 'nodeTemplates'),
          migratorTemplates: this.convertEdgesToArray(this.props.configuration.migratorTemplates.edges, 'migratorTemplates'),
          nugetFeed: this.props.configuration.nugetFeed,
          packages: this.convertEdgesToArray(this.props.configuration.packages.edges, 'packages'),
          seedAddresses: this.updateSeedAddresses(this.props.configuration.seedAddresses),
        },
      }
    }
  }

  getOptimisticResponse () {
    return {
      model: {
        id: this.props.nodeId,
        configuration: {
          nodeTemplates: this.props.configuration.nodeTemplates,
          migratorTemplates: this.props.configuration.migratorTemplates,
          nugetFeed: this.props.configuration.nugetFeed,
          packages: this.props.configuration.packages,
          seedAddresses: this.props.configuration.seedAddresses,
          id: this.props.configuration.id
        },
      },
    }
  }
}

