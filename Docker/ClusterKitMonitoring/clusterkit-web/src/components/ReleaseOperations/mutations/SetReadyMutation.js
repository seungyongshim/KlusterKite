import Relay from 'react-relay'

export default class SetReadyMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{ClusterKitNodeApi_clusterKitNodesApi_releases_setReady}`
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
      }
    `
  }

  getConfigs() {
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
          }
        `,
      ],
    }];
  }

  getVariables () {
    return {
      id: this.props.releaseId
    }
  }

  getOptimisticResponse () {
    return {
      result: {
        result: true
      }
    }
  }
}

